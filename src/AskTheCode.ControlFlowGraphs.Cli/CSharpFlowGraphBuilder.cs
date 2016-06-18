using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class CSharpFlowGraphBuilder
    {
        public const int A = 1;

        private BuilderSyntaxVisitor visitor;

        public CSharpFlowGraphBuilder(MethodDeclarationSyntax methodSyntax)
        {
            this.visitor = new BuilderSyntaxVisitor(this);

            var initialNode = new BuildNode(methodSyntax);
            this.Nodes.Add(initialNode);
            this.ReadyQueue.Enqueue(initialNode);
        }

        internal HashSet<BuildNode> Nodes { get; } = new HashSet<BuildNode>();

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

                        // TODO: Consider creating a new visitor so that its CurrentNode is not changed when awaited    
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
                this.ReenqueueCurrentNode(elseSyntax.Statement);

                return Task.CompletedTask;
            }

            public override Task VisitWhileStatement(WhileStatementSyntax whileSyntax)
            {
                // TODO: Consider putting this validation in a helper method and throwing an exception
                var outEdge = this.CurrentNode.OutgoingEdges.Single();
                Contract.Assert(outEdge.ValueCondition == null);

                this.CurrentNode.OutgoingEdges.Clear();
                var condition = this.ReenqueueCurrentNode(whileSyntax.Condition);
                var statement = this.EnqueueNode(whileSyntax.Statement);
                condition.AddEdge(statement, ExpressionFactory.True);
                statement.AddEdge(condition);

                this.CurrentNode.OutgoingEdges.Add(outEdge.WithValueCondition(ExpressionFactory.False));

                return Task.CompletedTask;
            }

            public override Task VisitParenthesizedExpression(ParenthesizedExpressionSyntax syntax)
            {
                this.ReenqueueCurrentNode(syntax.Expression);

                // TODO: Expression value processing
                return Task.CompletedTask;
            }

            public override Task VisitBinaryExpression(BinaryExpressionSyntax syntax)
            {
                // TODO: Check whether are the operators not overloaded
                //       (either implement it or show a warning)
                // TODO: Expression value processing
                switch (syntax.Kind())
                {
                    case SyntaxKind.LogicalOrExpression:
                        return this.ProcessLogicalOrExpression(syntax);
                    case SyntaxKind.LogicalAndExpression:
                        return this.ProcessLogicalAndExpression(syntax);

                    case SyntaxKind.BitwiseOrExpression:
                        // TODO (beware the special case if used on bool)
                        break;
                    case SyntaxKind.BitwiseAndExpression:
                        // TODO (beware the special case if used on bool)
                        break;
                    case SyntaxKind.ExclusiveOrExpression:
                        // TODO (beware the special case if used on bool)
                        break;

                    case SyntaxKind.None:
                    default:
                        return Task.CompletedTask;
                }

                // TODO: Remove when there are no breaks in the switch statement
                return Task.CompletedTask;
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
