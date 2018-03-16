using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class ExpressionDepthBuilderVisitor : StatementDepthBuilderVisitor
    {
        public ExpressionDepthBuilderVisitor(IBuildingContext context)
            : base(context)
        {
        }

        public sealed override void VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            this.Context.CurrentNode.Syntax = syntax.Expression;
            this.Context.CurrentNode.DisplayNode = this.Context.AddDisplayNode(syntax.Expression.Span);
            this.Visit(syntax.Expression);
        }

        public sealed override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax syntax)
        {
            this.Context.CurrentNode.Syntax = syntax.Declaration;
            this.Context.CurrentNode.DisplayNode = this.Context.AddDisplayNode(syntax.Declaration.Span);
            this.Visit(syntax.Declaration);
        }

        public sealed override void VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            this.ProcessSequentially(syntax.Variables, DisplayNodeConfig.Inherit);
        }

        public sealed override void VisitEqualsValueClause(EqualsValueClauseSyntax syntax)
        {
            this.Visit(syntax.Value);
        }

        public sealed override void VisitIdentifierName(IdentifierNameSyntax nameSyntax)
        {
            var valueModel = this.Context.TryGetModel(nameSyntax);
            if (valueModel != null)
            {
                Contract.Assert(this.Context.CurrentNode.ValueModel == null);
                this.Context.CurrentNode.ValueModel = valueModel;
            }
        }

        public sealed override void VisitVariableDeclarator(VariableDeclaratorSyntax declaratorSyntax)
        {
            var variableModel = this.Context.TryGetModel(declaratorSyntax);
            if (variableModel == null)
            {
                return;
            }

            if (declaratorSyntax.Initializer != null)
            {
                Contract.Assert(this.Context.CurrentNode.VariableModel == null);
                this.Context.CurrentNode.VariableModel = variableModel;
                this.Context.ReenqueueCurrentNode(declaratorSyntax.Initializer);
                this.Context.CurrentNode.LabelOverride = declaratorSyntax;
            }
        }

        // TODO: Handle also operations such as +=
        public sealed override void VisitAssignmentExpression(AssignmentExpressionSyntax assignmentSyntax)
        {
            var leftModel = this.Context.TryGetModel(assignmentSyntax.Left);
            if (leftModel == null)
            {
                return;
            }

            if (this.Context.CurrentNode.VariableModel == null)
            {
                this.Context.CurrentNode.VariableModel = leftModel;
                this.Context.CurrentNode.LabelOverride = assignmentSyntax;
                this.Context.ReenqueueCurrentNode(assignmentSyntax.Right);
            }
            else
            {
                // Nested assignments - from the view of the inner one
                var innerAssignment = this.Context.PrependCurrentNode(
                    assignmentSyntax.Right,
                    DisplayNodeConfig.Inherit);

                this.Context.CurrentNode.ValueModel = leftModel;
                innerAssignment.VariableModel = leftModel;
            }
        }

        public sealed override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            this.Context.CurrentNode.Syntax = syntax.Expression;
            this.Visit(syntax.Expression);
        }

        public sealed override void VisitLiteralExpression(LiteralExpressionSyntax literalSyntax)
        {
            Contract.Requires(this.Context.CurrentNode.ValueModel == null);

            var valueModel = this.Context.TryGetValueModel(literalSyntax);
            if (valueModel != null)
            {
                this.Context.CurrentNode.ValueModel = valueModel;
            }
        }

        // TODO: Handle ++ operators in a proper way
        public sealed override void VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax expressionSyntax)
        {
            this.ProcessOperation(expressionSyntax, expressionSyntax.Operand);
        }

        public sealed override void VisitBinaryExpression(BinaryExpressionSyntax expressionSyntax)
        {
            // TODO: Check whether are the operators not overloaded
            //       (either implement it or show a warning)
            switch (expressionSyntax.Kind())
            {
                case SyntaxKind.LogicalOrExpression:
                    this.ProcessLogicalOrExpression(expressionSyntax);
                    return;
                case SyntaxKind.LogicalAndExpression:
                    this.ProcessLogicalAndExpression(expressionSyntax);
                    return;

                case SyntaxKind.None:
                    return;
                default:
                    this.ProcessOperation(expressionSyntax, expressionSyntax.Left, expressionSyntax.Right);
                    return;
            }
        }

        public sealed override void VisitInvocationExpression(InvocationExpressionSyntax invocationSyntax)
        {
            var arguments = invocationSyntax.ArgumentList.Arguments.Select(arg => arg.Expression).ToArray();
            this.ProcessOperation(invocationSyntax, arguments);
        }

        private void ProcessOperation(ExpressionSyntax expressionSyntax, params ExpressionSyntax[] arguments)
        {
            var expressionSymbol = this.Context.SemanticModel.GetSymbolInfo(expressionSyntax).Symbol as IMethodSymbol;

            if (expressionSymbol == null)
            {
                return;
            }

            // Assertions are simplified to a simple assignment to a boolean temporary variable
            if (AssertionMethodRecognizer.IsAssertionMethod(expressionSymbol))
            {
                // Display the whole call of the assertion method
                this.Context.CurrentNode.LabelOverride = expressionSyntax;

                this.Context.CurrentNode.VariableModel = this.Context.TryCreateTemporaryVariableModel(arguments[0]);
                this.Context.ReenqueueCurrentNode(arguments[0]);
                return;
            }

            var argumentModels = new List<ITypeModel>();
            foreach (var argument in arguments)
            {
                var model = this.ProcessArgument(argument);
                argumentModels.Add(model);
            }

            bool modelled = false;

            if (argumentModels.All(model => model != null))
            {
                var factory = this.Context.ModelManager.TryGetFactory(expressionSymbol.ContainingType);

                if (factory != null)
                {
                    var modelContext = this.Context.GetModellingContext();
                    factory.ModelOperation(modelContext, expressionSymbol, argumentModels);

                    modelled = !modelContext.IsUnsupported;
                }
            }

            if (!modelled)
            {
                this.Context.CurrentNode.BorderData = new BorderData(
                    BorderDataKind.MethodCall,
                    expressionSymbol,
                    argumentModels);
            }
        }

        private ITypeModel ProcessArgument(ExpressionSyntax argument)
        {
            var argumentModel = this.Context.TryGetModel(argument);
            if (argumentModel == null)
            {
                var argumentType = this.Context.SemanticModel.GetTypeInfo(argument).Type;
                if (argumentType == null)
                {
                    return null;
                }

                var argumentFactory = this.Context.ModelManager.TryGetFactory(argumentType);
                if (argumentFactory == null)
                {
                    return null;
                }

                argumentModel = this.Context.CreateTemporaryVariableModel(argumentFactory, argumentType);

                var argumentComputation = this.Context.PrependCurrentNode(argument, DisplayNodeConfig.Inherit);
                argumentComputation.VariableModel = argumentModel;
            }

            return argumentModel;
        }

        private void ProcessLogicalAndExpression(BinaryExpressionSyntax andSyntax)
        {
            var left = this.Context.ReenqueueCurrentNode(andSyntax.Left);
            var right = this.Context.EnqueueNode(andSyntax.Right, DisplayNodeConfig.Inherit);
            right.VariableModel = left.VariableModel;

            left.LabelOverride = null;

            BuildEdge outEdge, outTrueEdge, outFalseEdge;
            if (left.TryGetSingleEdge(out outEdge))
            {
                left.OutgoingEdges.Clear();
                left.AddEdge(outEdge.WithValueCondition(ExpressionFactory.False));
                right.AddEdge(outEdge);
            }
            else if (left.TryGetTwoBooleanEdges(out outTrueEdge, out outFalseEdge))
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

            return;
        }

        private void ProcessLogicalOrExpression(BinaryExpressionSyntax orSyntax)
        {
            var left = this.Context.ReenqueueCurrentNode(orSyntax.Left);
            var right = this.Context.EnqueueNode(orSyntax.Right, DisplayNodeConfig.Inherit);
            right.VariableModel = left.VariableModel;

            left.LabelOverride = null;

            BuildEdge outEdge, outTrueEdge, outFalseEdge;
            if (left.TryGetSingleEdge(out outEdge))
            {
                left.OutgoingEdges.Clear();
                left.AddEdge(outEdge.WithValueCondition(ExpressionFactory.True));
                right.AddEdge(outEdge);
            }
            else if (left.TryGetTwoBooleanEdges(out outTrueEdge, out outFalseEdge))
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

            return;
        }
    }
}
