using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal enum GraphDepth
    {
        Statement,
        Expression,
        Value
    }

    internal enum DisplayNodeConfig
    {
        Ignore,
        Inherit,
        CreateNew
    }

    // TODO: Consider putting to CSharp namespace together with the related classes
    internal class CSharpGraphBuilder
    {
        // TODO: Move modelManager to CSharpFlowGraphProvider
        private readonly TypeModelManager modelManager;
        private readonly SemanticModel semanticModel;
        private readonly MethodDeclarationSyntax methodSyntax;

        private readonly Queue<BuildNode> readyQueue = new Queue<BuildNode>();
        private readonly HashSet<BuildingContext> pending = new HashSet<BuildingContext>();
        private readonly DocumentId documentId;

        public CSharpGraphBuilder(
            TypeModelManager modelManager,
            DocumentId documentId,
            SemanticModel semanticModel,
            MethodDeclarationSyntax methodSyntax)
        {
            Contract.Requires<ArgumentNullException>(modelManager != null, nameof(modelManager));
            Contract.Requires<ArgumentNullException>(documentId != null, nameof(documentId));
            Contract.Requires<ArgumentNullException>(semanticModel != null, nameof(semanticModel));
            Contract.Requires<ArgumentNullException>(methodSyntax != null, nameof(methodSyntax));

            this.modelManager = modelManager;
            this.documentId = documentId;
            this.semanticModel = semanticModel;
            this.methodSyntax = methodSyntax;

            this.DisplayGraph = new DisplayGraph(this.documentId);
        }

        public DisplayGraph DisplayGraph { get; private set; }

        public async Task<BuildGraph> BuildAsync(GraphDepth depth = GraphDepth.Value)
        {
            this.readyQueue.Clear();
            this.pending.Clear();

            var graph = new BuildGraph(this.documentId, this.methodSyntax);
            this.readyQueue.Enqueue(graph.EnterNode);

            var context = new BuildingContext(this, graph);
            var visitor = CreateVisitor(context, depth);

            while (this.readyQueue.Count > 0 || this.pending.Count > 0)
            {
                if (this.readyQueue.Count > 0)
                {
                    var node = this.readyQueue.Dequeue();
                    Contract.Assert(context.PendingTask == null);

                    context.CurrentNode = node;
                    visitor.Visit(node.Syntax);

                    if (context.PendingTask != null)
                    {
                        this.pending.Add(context);

                        // Create new visitor and context so that CurrentNode is not changed when awaited
                        context = new BuildingContext(this, graph);
                        visitor = CreateVisitor(context, depth);
                    }
                }
                else if (this.pending.Count > 0)
                {
                    // TODO: Consider making this more effective
                    //       (e.g. directly access and remove the particular node)
                    await Task.WhenAny(this.pending.Select(item => item.PendingTask));
                    this.pending.RemoveWhere(item => item.PendingTask.IsCompleted);
                }

                // TODO: Handle the exceptions thrown in the tasks, if necessary
            }

            return graph;
        }

        private static BuilderVisitor CreateVisitor(BuildingContext context, GraphDepth depth)
        {
            switch (depth)
            {
                case GraphDepth.Statement:
                    return new StatementDepthBuilderVisitor(context);
                case GraphDepth.Expression:
                    return new ExpressionDepthBuilderVisitor(context);
                case GraphDepth.Value:
                    return new ValueDepthBuilderVisitor(context);
                default:
                    throw new ArgumentException();
            }
        }

        // TODO: Extract to interface (+ extension methods) and make private, change in the documentation otherwise
        public class BuildingContext : IBuildingContext
        {
            private CSharpGraphBuilder builder;

            public BuildingContext(CSharpGraphBuilder builder, BuildGraph graph)
            {
                Contract.Requires(builder != null);
                Contract.Requires(graph != null);

                this.builder = builder;
                this.Graph = graph;
            }

            public BuildNode CurrentNode { get; set; }

            public Task PendingTask { get; set; }

            public SemanticModel SemanticModel
            {
                get { return this.builder.semanticModel; }
            }

            public TypeModelManager ModelManager
            {
                get { return this.builder.modelManager; }
            }

            protected BuildGraph Graph { get; private set; }

            public ITypeModel TryGetModel(SyntaxNode syntax)
            {
                ISymbol symbol;
                switch (syntax.Kind())
                {
                    case SyntaxKind.TrueLiteralExpression:
                    case SyntaxKind.FalseLiteralExpression:
                    case SyntaxKind.NumericLiteralExpression:
                    case SyntaxKind.UnaryMinusExpression:
                    case SyntaxKind.UnaryPlusExpression:
                    case SyntaxKind.CharacterLiteralExpression:
                    case SyntaxKind.StringLiteralExpression:
                        return this.TryGetValueModel(syntax);

                    case SyntaxKind.Parameter:
                    case SyntaxKind.VariableDeclarator:
                        symbol = this.SemanticModel.GetDeclaredSymbol(syntax);
                        break;
                    default:
                        symbol = this.SemanticModel.GetSymbolInfo(syntax).Symbol;
                        break;
                }

                if (symbol == null)
                {
                    return null;
                }

                return this.TryGetDefinedVariableModel(symbol);
            }

            public IValueModel TryGetValueModel(SyntaxNode syntax)
            {
                var maybeValue = this.SemanticModel.GetConstantValue(syntax);
                if (!maybeValue.HasValue)
                {
                    return null;
                }

                var type = this.SemanticModel.GetTypeInfo(syntax).Type;
                if (type == null)
                {
                    return null;
                }

                var factory = this.ModelManager.TryGetFactory(type);
                if (factory == null)
                {
                    return null;
                }

                return factory.GetLiteralValueModel(type, maybeValue.Value);
            }

            // TODO: Consider using the FlowVariable directly
            public ITypeModel TryGetDefinedVariableModel(ISymbol symbol)
            {
                Contract.Requires(symbol != null);

                ITypeModel existingModel;
                if (this.Graph.DefinedVariableModels.TryGetValue(symbol, out existingModel))
                {
                    return existingModel;
                }

                VariableOrigin origin;
                ITypeSymbol type;

                switch (symbol.Kind)
                {
                    case SymbolKind.Local:
                        var localSymbol = (ILocalSymbol)symbol;
                        origin = VariableOrigin.Local;
                        type = localSymbol.Type;
                        break;
                    case SymbolKind.Parameter:
                        var parameterSymbol = (IParameterSymbol)symbol;
                        origin = VariableOrigin.Parameter;
                        type = parameterSymbol.Type;
                        break;
                    default:
                        return null;
                }

                var factory = this.ModelManager.TryGetFactory(type);
                if (factory == null)
                {
                    return null;
                }

                var createdModel = this.CreateVariableModel(factory, type, symbol, origin);
                this.Graph.DefinedVariableModels.Add(symbol, createdModel);

                return createdModel;
            }

            public ITypeModel TryCreateTemporaryVariableModel(SyntaxNode syntax)
            {
                // TODO: Consider working also with the ConvertedType somehow
                var type = this.SemanticModel.GetTypeInfo(syntax).Type;
                if (type == null)
                {
                    return null;
                }

                var factory = this.ModelManager.TryGetFactory(type);
                if (factory == null)
                {
                    return null;
                }

                return this.CreateTemporaryVariableModel(factory, type);
            }

            public ITypeModel CreateTemporaryVariableModel(ITypeModelFactory factory, ITypeSymbol type)
            {
                return this.CreateVariableModel(factory, type, null, VariableOrigin.Temporary);
            }

            public IModellingContext GetModellingContext()
            {
                return new BuilderModellingContext(this.builder, this.CurrentNode);
            }

            public BuildNode AddFinalNode(SyntaxNodeOrToken label, bool createDisplayNode = false)
            {
                BuildNode node;
                if (label.IsNode)
                {
                    node = this.Graph.AddNode(label.AsNode());
                }
                else
                {
                    Contract.Assert(label.IsToken);
                    node = this.Graph.AddNode(null);
                    node.LabelOverride = label;
                }

                if (createDisplayNode)
                {
                    node.DisplayNode = this.AddDisplayNode(label.Span);
                }

                return node;
            }

            public BuildNode EnqueueNode(SyntaxNode syntax, DisplayNodeConfig displayConfig = DisplayNodeConfig.Ignore)
            {
                var node = this.Graph.AddNode(syntax);
                this.builder.readyQueue.Enqueue(node);

                if (displayConfig == DisplayNodeConfig.CreateNew)
                {
                    node.DisplayNode = this.AddDisplayNode(syntax.Span);
                }
                else if (displayConfig == DisplayNodeConfig.Inherit)
                {
                    node.DisplayNode = this.CurrentNode.DisplayNode;
                }

                return node;
            }

            public BuildNode PrependCurrentNode(
                SyntaxNode prependedSyntax,
                DisplayNodeConfig displayConfig = DisplayNodeConfig.Ignore)
            {
                // The syntaxes will be swaped subsequently
                var prependedCurrent = this.ReenqueueCurrentNode(this.CurrentNode.Syntax);
                var newCurrent = this.AddFinalNode(prependedSyntax);

                // Swaping contents will change the outgoing edges, i.a.
                newCurrent.SwapContents(prependedCurrent);
                prependedCurrent.AddEdge(newCurrent);

                this.CurrentNode = newCurrent;

                if (displayConfig == DisplayNodeConfig.CreateNew)
                {
                    prependedCurrent.DisplayNode = this.AddDisplayNode(prependedSyntax.Span);
                }
                else if (displayConfig == DisplayNodeConfig.Inherit)
                {
                    prependedCurrent.DisplayNode = this.CurrentNode.DisplayNode;
                }

                return prependedCurrent;
            }

            public BuildNode ReenqueueCurrentNode(SyntaxNode syntaxUpdate, bool createDisplayNode = false)
            {
                this.CurrentNode.Syntax = syntaxUpdate;
                this.builder.readyQueue.Enqueue(this.CurrentNode);

                if (createDisplayNode)
                {
                    this.CurrentNode.DisplayNode = this.AddDisplayNode(syntaxUpdate.Span);
                }

                return this.CurrentNode;
            }

            public DisplayNode AddDisplayNode(TextSpan span)
            {
                return this.builder.DisplayGraph.AddNode(span);
            }

            public void DelayCompletion(Task pendingTask)
            {
                Contract.Requires(this.PendingTask == null);

                this.PendingTask = pendingTask;
            }

            private ITypeModel CreateVariableModel(
                ITypeModelFactory factory,
                ITypeSymbol type,
                ISymbol symbol,
                VariableOrigin origin)
            {
                var variables = factory.GetExpressionSortRequirements(type)
                    .Select(sort => this.Graph.AddVariable(sort, symbol, origin))
                    .ToArray();

                return factory.GetExpressionModel(type, variables);
            }
        }

        private class BuilderModellingContext : IModellingContext
        {
            private CSharpGraphBuilder owner;
            private BuildNode node;

            public BuilderModellingContext(CSharpGraphBuilder owner, BuildNode node)
            {
                Contract.Requires(owner != null);
                Contract.Requires(node != null);

                this.owner = owner;
                this.node = node;
            }

            public bool IsUnsupported { get; private set; }

            public void SetUnsupported()
            {
                this.IsUnsupported = true;
            }

            public void AddAssignment(Variable variable, Expression value)
            {
                throw new NotImplementedException();
            }

            public void AddExceptionThrow(BoolHandle condition, Type exceptionType)
            {
                throw new NotImplementedException();
            }

            public void SetResultValue(ITypeModel valueModel)
            {
                Contract.Assert(this.node.ValueModel == null);

                this.node.ValueModel = valueModel;
            }
        }
    }
}
