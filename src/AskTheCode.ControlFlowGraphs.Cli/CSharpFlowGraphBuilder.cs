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

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class CSharpFlowGraphBuilder
    {
        private BuilderSyntaxVisitor visitor;
        private BuildVariableId.Provider variableIdProvider = new BuildVariableId.Provider();

        // TODO: Consider moving its creation to CSharpFlowGraphProvider once it is completed (beware the thread safety)
        private TypeModelManager modelManager = new TypeModelManager();

        private SemanticModel semanticModel;

        public CSharpFlowGraphBuilder(SemanticModel semanticModel, MethodDeclarationSyntax methodSyntax)
        {
            Contract.Requires<ArgumentNullException>(methodSyntax != null, nameof(methodSyntax));

            this.visitor = new BuilderSyntaxVisitor(this);
            this.semanticModel = semanticModel;

            var initialNode = new BuildNode(methodSyntax);
            this.Nodes.Add(initialNode);
            this.ReadyQueue.Enqueue(initialNode);
        }

        internal HashSet<BuildNode> Nodes { get; } = new HashSet<BuildNode>();

        internal List<BuildVariable> Variables { get; } = new List<BuildVariable>();

        internal Dictionary<ISymbol, ITypeModel> DefinedVariableModels { get; } = new Dictionary<ISymbol, ITypeModel>();

        private Queue<BuildNode> ReadyQueue { get; } = new Queue<BuildNode>();

        private HashSet<BuildNode> Pending { get; } = new HashSet<BuildNode>();

        public async Task BuildAsync()
        {
            while (this.ReadyQueue.Count > 0 || this.Pending.Count > 0)
            {
                if (this.ReadyQueue.Count > 0)
                {
                    var node = this.ReadyQueue.Dequeue();
                    Contract.Assert(node.PendingTask == null);
                    Contract.Assert(node.Syntax.IsNode);

                    this.visitor.CurrentNode = node;
                    var task = this.visitor.Visit(node.Syntax.AsNode());
                    if (!task.IsCompleted)
                    {
                        node.PendingTask = task;
                        this.Pending.Add(node);

                        // Create a new visitor so that its CurrentNode is not changed when awaited
                        this.visitor = new BuilderSyntaxVisitor(this);
                    }
                }
                else if (this.Pending.Count > 0)
                {
                    // TODO: Consider making this more effective
                    //       (e.g. directly access and remove the particular node)
                    await Task.WhenAny(this.Pending.Select(node => node.PendingTask));
                    this.Pending.RemoveWhere(node => node.PendingTask.IsCompleted);
                }

                // TODO: Handle the exceptions thrown in the tasks, if necessary
            }
        }

        private BuildVariable AddVariable(Sort sort, ISymbol symbol, VariableOrigin origin)
        {
            var variableId = this.variableIdProvider.GenerateNewId();
            var variable = new BuildVariable(variableId, sort, symbol, origin);
            this.Variables.Add(variable);
            Contract.Assert(variableId.Value == this.Variables.IndexOf(variable));

            return variable;
        }

        private ITypeModel TryGetDefinedVariableModel(SyntaxNode syntax)
        {
            var symbol = this.semanticModel.GetSymbolInfo(syntax).Symbol;
            if (symbol == null)
            {
                return null;
            }

            return this.TryGetDefinedVariableModel(symbol);
        }

        // TODO: Consider using the FlowVariable directly
        private ITypeModel TryGetDefinedVariableModel(ISymbol symbol)
        {
            Contract.Requires(symbol != null);

            ITypeModel existingModel;
            if (this.DefinedVariableModels.TryGetValue(symbol, out existingModel))
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

            var factory = this.modelManager.TryGetFactory(type);
            if (factory == null)
            {
                return null;
            }

            var createdModel = this.CreateVariableModel(factory, type, symbol, origin);
            this.DefinedVariableModels.Add(symbol, createdModel);

            return createdModel;
        }

        private ITypeModel CreateTemporaryVariableModel(ITypeModelFactory factory, ITypeSymbol type)
        {
            return this.CreateVariableModel(factory, type, null, VariableOrigin.Temporary);
        }

        // TODO: Consider hiding this as an implementation after refactoring
        private ITypeModel CreateVariableModel(
            ITypeModelFactory factory,
            ITypeSymbol type,
            ISymbol symbol,
            VariableOrigin origin)
        {
            var variables = factory.GetExpressionSortRequirements(type)
                .Select(sort => this.AddVariable(sort, symbol, origin))
                .ToArray();

            return factory.GetVariableModel(type, variables);
        }

        private class BuilderModellingContext : IModellingContext
        {
            private CSharpFlowGraphBuilder owner;
            private BuildNode node;

            public BuilderModellingContext(CSharpFlowGraphBuilder owner, BuildNode node)
            {
                Contract.Requires(owner != null);
                Contract.Requires(node != null);

                this.owner = owner;
                this.node = node;
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

        private class BuilderSyntaxVisitor : CSharpSyntaxVisitor<Task>
        {
            private CSharpFlowGraphBuilder owner;

            public BuilderSyntaxVisitor(CSharpFlowGraphBuilder owner)
            {
                this.owner = owner;
            }

            public BuildNode CurrentNode { get; set; }

            public override Task DefaultVisit(SyntaxNode node)
            {
                return Task.CompletedTask;
            }

            public override Task VisitMethodDeclaration(MethodDeclarationSyntax methodSyntax)
            {
                Contract.Requires(this.CurrentNode.OutgoingEdges.Count == 0);

                var enter = this.EnqueueNode(methodSyntax.ParameterList);
                var body = this.EnqueueNode(methodSyntax.Body);
                enter.AddEdge(body);

                if ((methodSyntax.ReturnType as PredefinedTypeSyntax).Keyword.Text == "void")
                {
                    var implicitReturn = this.AddFinalNode(methodSyntax.Body.CloseBraceToken);
                    body.AddEdge(implicitReturn);

                    // TODO: Add also ReturnFlowNode here
                }

                this.RemoveNode(this.CurrentNode);

                return Task.CompletedTask;
            }

            public override Task VisitReturnStatement(ReturnStatementSyntax returnSyntax)
            {
                this.CurrentNode.OutgoingEdges.Clear();

                // TODO: Handle also the return value computation
                return Task.CompletedTask;
            }

            public override Task VisitBlock(BlockSyntax blockSyntax)
            {
                var outEdge = this.CurrentNode.OutgoingEdges.SingleOrDefault();
                Contract.Assert(outEdge?.ValueCondition == null);

                // TODO: Consider merging with the following node in the case of empty block
                //       (or leave it to the FlowGraph construction)
                if (blockSyntax.Statements.Count > 0)
                {
                    this.CurrentNode.OutgoingEdges.Clear();
                    var precedingStatement = this.ReenqueueCurrentNode(blockSyntax.Statements.First());

                    for (int i = 1; i < blockSyntax.Statements.Count; i++)
                    {
                        var currentStatement = this.EnqueueNode(blockSyntax.Statements[i]);
                        precedingStatement.AddEdge(currentStatement);

                        precedingStatement = currentStatement;
                    }

                    if (outEdge != null)
                    {
                        precedingStatement.AddEdge(outEdge);
                    }
                }

                return Task.CompletedTask;
            }

            public override Task VisitIfStatement(IfStatementSyntax ifSyntax)
            {
                var outEdge = this.CurrentNode.OutgoingEdges.SingleOrDefault();
                Contract.Assert(outEdge?.ValueCondition == null);

                this.CurrentNode.OutgoingEdges.Clear();
                var condition = this.ReenqueueCurrentNode(ifSyntax.Condition);
                var statement = this.EnqueueNode(ifSyntax.Statement);
                condition.AddEdge(statement, ExpressionFactory.True);

                if (outEdge != null)
                {
                    statement.AddEdge(outEdge);
                }

                if (ifSyntax.Else != null)
                {
                    var elseBody = this.EnqueueNode(ifSyntax.Else);
                    condition.AddEdge(elseBody, ExpressionFactory.False);

                    if (outEdge != null)
                    {
                        elseBody.AddEdge(outEdge);
                    }
                }
                else
                {
                    if (outEdge == null)
                    {
                        // TODO: Add a message and put to resources
                        //       (probably related to: "Not all code paths return a value")
                        throw new InvalidOperationException();
                    }

                    condition.AddEdge(outEdge.To, ExpressionFactory.False);
                }

                return Task.CompletedTask;
            }

            public override Task VisitElseClause(ElseClauseSyntax elseSyntax)
            {
                return this.Visit(elseSyntax.Statement);
            }

            public override Task VisitWhileStatement(WhileStatementSyntax whileSyntax)
            {
                var outEdge = this.GetSingleEdge(this.CurrentNode);

                this.CurrentNode.OutgoingEdges.Clear();
                var condition = this.ReenqueueCurrentNode(whileSyntax.Condition);
                var statement = this.EnqueueNode(whileSyntax.Statement);
                condition.AddEdge(statement, ExpressionFactory.True);
                statement.AddEdge(condition);

                this.CurrentNode.OutgoingEdges.Add(outEdge.WithValueCondition(ExpressionFactory.False));

                return Task.CompletedTask;
            }

            public override Task VisitExpressionStatement(ExpressionStatementSyntax node)
            {
                return this.Visit(node.Expression);
            }

            public override Task VisitAssignmentExpression(AssignmentExpressionSyntax assignmentSyntax)
            {
                var leftModel = this.owner.TryGetDefinedVariableModel(assignmentSyntax.Left);
                if (leftModel == null)
                {
                    return Task.CompletedTask;
                }

                if (this.CurrentNode.VariableModel == null)
                {
                    this.CurrentNode.VariableModel = leftModel;
                    this.ReenqueueCurrentNode(assignmentSyntax.Right);
                }
                else
                {
                    // This is the case of nested assignments
                    var innerAssignment = this.ReenqueueCurrentNode(assignmentSyntax.Right);
                    var outerAssignment = this.AddFinalNode(assignmentSyntax);
                    innerAssignment.SwapEdges(outerAssignment);
                    innerAssignment.AddEdge(outerAssignment);
                    innerAssignment.SwapVariableModel(outerAssignment);
                    innerAssignment.VariableModel = leftModel;
                }

                return Task.CompletedTask;
            }

            public override Task VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
            {
                this.ReenqueueCurrentNode(syntax.Expression);

                // TODO: Expression value processing
                return Task.CompletedTask;
            }

            public override Task VisitBinaryExpression(BinaryExpressionSyntax expressionSyntax)
            {
                // TODO: Check whether are the operators not overloaded
                //       (either implement it or show a warning)
                // TODO: Expression value processing
                switch (expressionSyntax.Kind())
                {
                    case SyntaxKind.LogicalOrExpression:
                        return this.ProcessLogicalOrExpression(expressionSyntax);
                    case SyntaxKind.LogicalAndExpression:
                        return this.ProcessLogicalAndExpression(expressionSyntax);

                    case SyntaxKind.None:
                        return Task.CompletedTask;
                    default:
                        break;
                }

                var expressionSymbol = this.owner.semanticModel.GetSymbolInfo(expressionSyntax).Symbol as IMethodSymbol;
                if (expressionSymbol == null)
                {
                    return Task.CompletedTask;
                }

                var factory = this.owner.modelManager.TryGetFactory(expressionSymbol.ReturnType);
                if (factory == null)
                {
                    return Task.CompletedTask;
                }

                var outEdges = this.CurrentNode.OutgoingEdges.ToArray();
                this.CurrentNode.OutgoingEdges.Clear();

                ITypeModel leftModel = this.ProcessArgument(
                    expressionSyntax,
                    expressionSyntax.Left,
                    expressionSymbol.Parameters[0].Type);

                ITypeModel rightModel = this.ProcessArgument(
                    expressionSyntax,
                    expressionSyntax.Right,
                    expressionSymbol.Parameters[1].Type);

                this.CurrentNode.OutgoingEdges.AddRange(outEdges);

                if (leftModel != null && rightModel != null)
                {
                    var modelContext = new BuilderModellingContext(this.owner, this.CurrentNode);
                    factory.ModelOperation(modelContext, expressionSymbol, new[] { leftModel, rightModel });
                }

                return Task.CompletedTask;
            }

            private ITypeModel ProcessArgument(
                ExpressionSyntax expressionSyntax,
                ExpressionSyntax argument,
                ITypeSymbol argumentType)
            {
                var argumentModel = this.owner.TryGetDefinedVariableModel(argument);
                if (argumentModel == null)
                {
                    var argumentFactory = this.owner.modelManager.TryGetFactory(argumentType);
                    if (argumentFactory != null)
                    {
                        argumentModel = this.owner.CreateTemporaryVariableModel(argumentFactory, argumentType);

                        var argumentComputation = this.ReenqueueCurrentNode(argument);
                        this.CurrentNode = this.AddFinalNode(expressionSyntax);
                        argumentComputation.AddEdge(this.CurrentNode);

                        if (argumentComputation.VariableModel != null)
                        {
                            argumentComputation.SwapVariableModel(this.CurrentNode);
                        }

                        argumentComputation.VariableModel = argumentModel;
                    }
                }

                return argumentModel;
            }

            private Task ProcessLogicalAndExpression(BinaryExpressionSyntax andSyntax)
            {
                var left = this.ReenqueueCurrentNode(andSyntax.Left);
                var right = this.EnqueueNode(andSyntax.Right);

                BuildEdge outEdge, outTrueEdge, outFalseEdge;
                if (this.TryGetSingleEdge(left, out outEdge))
                {
                    left.OutgoingEdges.Clear();
                    left.AddEdge(outEdge.WithValueCondition(ExpressionFactory.False));
                    right.AddEdge(outEdge);
                }
                else if (this.TryGetTwoBooleanEdges(left, out outTrueEdge, out outFalseEdge))
                {
                    left.OutgoingEdges.Remove(outTrueEdge);
                    right.OutgoingEdges.Add(outTrueEdge);
                    right.OutgoingEdges.Add(outFalseEdge);
                }
                else
                {
                    // TODO: Prevent this case in the switch statement if switched on boolean
                    // TODO: Add a message and put to resources
                    Contract.Assert(false);
                }

                left.AddEdge(right, ExpressionFactory.True);

                return Task.CompletedTask;
            }

            private Task ProcessLogicalOrExpression(BinaryExpressionSyntax orSyntax)
            {
                var left = this.ReenqueueCurrentNode(orSyntax.Left);
                var right = this.EnqueueNode(orSyntax.Right);

                BuildEdge outEdge, outTrueEdge, outFalseEdge;
                if (this.TryGetSingleEdge(left, out outEdge))
                {
                    left.OutgoingEdges.Clear();
                    left.AddEdge(outEdge.WithValueCondition(ExpressionFactory.True));
                    right.AddEdge(outEdge);
                }
                else if (this.TryGetTwoBooleanEdges(left, out outTrueEdge, out outFalseEdge))
                {
                    left.OutgoingEdges.Remove(outFalseEdge);
                    right.OutgoingEdges.Add(outTrueEdge);
                    right.OutgoingEdges.Add(outFalseEdge);
                }
                else
                {
                    // TODO: Prevent this case in the switch statement if switched on boolean
                    // TODO: Add a message and put to resources
                    Contract.Assert(false);
                }

                left.AddEdge(right, ExpressionFactory.False);

                return Task.CompletedTask;
            }

            private BuildEdge GetSingleEdge(BuildNode node)
            {
                if (node.OutgoingEdges.Count != 1)
                {
                    // TODO: Add a message and put to resources
                    throw new InvalidOperationException();
                }

                var edge = node.OutgoingEdges.Single();
                Contract.Assert(edge.ValueCondition == null);

                return edge;
            }

            private bool TryGetSingleEdge(BuildNode node, out BuildEdge edge)
            {
                if (node.OutgoingEdges.Count == 1)
                {
                    edge = node.OutgoingEdges.Single();
                    Contract.Assert(edge.ValueCondition == null);

                    return true;
                }
                else
                {
                    edge = null;

                    return false;
                }
            }

            private bool TryGetTwoBooleanEdges(BuildNode node, out BuildEdge trueEdge, out BuildEdge falseEdge)
            {
                if (node.OutgoingEdges.Count == 2)
                {
                    trueEdge = node.OutgoingEdges.First(edge => edge.ValueCondition == ExpressionFactory.True);
                    falseEdge = node.OutgoingEdges.First(edge => edge.ValueCondition == ExpressionFactory.False);

                    return (trueEdge != null && falseEdge != null);
                }
                else
                {
                    trueEdge = null;
                    falseEdge = null;

                    return false;
                }
            }

            private BuildNode AddFinalNode(SyntaxNodeOrToken syntax)
            {
                var node = new BuildNode(syntax);
                this.owner.Nodes.Add(node);

                return node;
            }

            private BuildNode EnqueueNode(SyntaxNode syntax)
            {
                var node = new BuildNode(syntax);
                this.owner.Nodes.Add(node);
                this.owner.ReadyQueue.Enqueue(node);

                return node;
            }

            private BuildNode ReenqueueCurrentNode(SyntaxNode syntaxUpdate)
            {
                this.CurrentNode.Syntax = syntaxUpdate;
                this.CurrentNode.PendingTask = null;
                this.owner.ReadyQueue.Enqueue(this.CurrentNode);

                return this.CurrentNode;
            }

            private void RemoveNode(BuildNode node)
            {
                this.owner.Nodes.Remove(node);
            }
        }
    }
}
