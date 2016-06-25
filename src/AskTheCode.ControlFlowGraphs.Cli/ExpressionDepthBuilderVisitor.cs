using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using AskTheCode.SmtLibStandard.Handles;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class ExpressionDepthBuilderVisitor : StatementDepthBuilderVisitor
    {
        public ExpressionDepthBuilderVisitor(CSharpFlowGraphBuilder.BuildingContext context)
            : base(context)
        {
        }

        public sealed override void VisitExpressionStatement(ExpressionStatementSyntax syntax)
        {
            this.Context.CurrentNode.Syntax = syntax.Expression;
            this.Visit(syntax.Expression);
        }

        public sealed override void VisitLocalDeclarationStatement(LocalDeclarationStatementSyntax syntax)
        {
            this.Context.CurrentNode.Syntax = syntax.Declaration;
            this.Visit(syntax.Declaration);
        }

        public sealed override void VisitVariableDeclaration(VariableDeclarationSyntax syntax)
        {
            this.ProcessSequentially(syntax.Variables);
        }

        public sealed override void VisitEqualsValueClause(EqualsValueClauseSyntax syntax)
        {
            this.Visit(syntax.Value);
        }

        public sealed override void VisitIdentifierName(IdentifierNameSyntax nameSyntax)
        {
            var valueModel = this.Context.TryGetDefinedVariableModel(nameSyntax);
            if (valueModel != null)
            {
                Contract.Assert(this.Context.CurrentNode.ValueModel == null);
                this.Context.CurrentNode.ValueModel = valueModel;
            }
        }

        public sealed override void VisitVariableDeclarator(VariableDeclaratorSyntax declaratorSyntax)
        {
            var variableModel = this.Context.TryGetDefinedVariableModel(declaratorSyntax);
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

        public sealed override void VisitAssignmentExpression(AssignmentExpressionSyntax assignmentSyntax)
        {
            var leftModel = this.Context.TryGetDefinedVariableModel(assignmentSyntax.Left);
            if (leftModel == null)
            {
                return;
            }

            // TODO: Handle also operations such as +=

            if (this.Context.CurrentNode.VariableModel == null)
            {
                this.Context.CurrentNode.VariableModel = leftModel;
                this.Context.CurrentNode.LabelOverride = assignmentSyntax;
                this.Context.ReenqueueCurrentNode(assignmentSyntax.Right);
            }
            else
            {
                // Nested assignments - from the view of the inner one
                var innerAssignment = this.Context.PrependCurrentNode(assignmentSyntax.Right);


                this.Context.CurrentNode.ValueModel = leftModel;
                innerAssignment.VariableModel = leftModel;
            }
        }

        public sealed override void VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
        {
            this.Context.CurrentNode.Syntax = syntax.Expression;
            this.Visit(syntax.Expression);
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
                    this.ProcessOperation(expressionSyntax);
                    return;
            }

        }

        private void ProcessOperation(BinaryExpressionSyntax expressionSyntax)
        {
            var expressionSymbol =
                            this.Context.SemanticModel.GetSymbolInfo(expressionSyntax).Symbol as IMethodSymbol;

            if (expressionSymbol == null)
            {
                return;
            }

            var factory = this.Context.ModelManager.TryGetFactory(expressionSymbol.ContainingType);
            if (factory == null)
            {
                return;
            }

            var leftModel = this.ProcessArgument(
                expressionSyntax.Left,
                expressionSymbol.Parameters[0].Type);

            var rightModel = this.ProcessArgument(
                expressionSyntax.Right,
                expressionSymbol.Parameters[1].Type);


            if (leftModel != null && rightModel != null)
            {
                var modelContext = this.Context.GetModellingContext();
                factory.ModelOperation(modelContext, expressionSymbol, new[] { leftModel, rightModel });
            }
        }

        private ITypeModel ProcessArgument(
            ExpressionSyntax argument,
            ITypeSymbol argumentType)
        {
            var argumentModel = this.Context.TryGetDefinedVariableModel(argument);
            if (argumentModel == null)
            {
                var argumentFactory = this.Context.ModelManager.TryGetFactory(argumentType);
                if (argumentFactory != null)
                {
                    argumentModel = this.Context.CreateTemporaryVariableModel(argumentFactory, argumentType);

                    var argumentComputation = this.Context.PrependCurrentNode(argument);

                    argumentComputation.VariableModel = argumentModel;
                }
            }

            return argumentModel;
        }

        private void ProcessLogicalAndExpression(BinaryExpressionSyntax andSyntax)
        {
            var left = this.Context.ReenqueueCurrentNode(andSyntax.Left);
            var right = this.Context.EnqueueNode(andSyntax.Right);
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
            var right = this.Context.EnqueueNode(orSyntax.Right);
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
