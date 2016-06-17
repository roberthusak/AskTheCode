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
                var body = this.EnqueueNode(ifSyntax.Statement);
                condition.AddEdge(body, ExpressionFactory.True);

                if (outEdge != null)
                {
                    body.AddEdge(outEdge);
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
