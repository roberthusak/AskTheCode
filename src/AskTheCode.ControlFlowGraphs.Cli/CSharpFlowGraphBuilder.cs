using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var initialNode = new NodeStub(methodSyntax);
            this.NodeStubs.Add(initialNode);
            this.ReadyQueue.Enqueue(initialNode);
        }

        internal HashSet<NodeStub> NodeStubs { get; } = new HashSet<NodeStub>();

        private Queue<NodeStub> ReadyQueue { get; } = new Queue<NodeStub>();

        private HashSet<NodeStub> Pending { get; } = new HashSet<NodeStub>();

        public async Task BuildAsync()
        {
            while (this.ReadyQueue.Count > 0 || this.Pending.Count > 0)
            {
                if (this.ReadyQueue.Count > 0)
                {
                    var node = this.ReadyQueue.Dequeue();
                    Contract.Assert(node.PendingTask == null || node.PendingTask.IsCompleted);
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

            public NodeStub CurrentNode { get; set; }

            public override Task VisitMethodDeclaration(MethodDeclarationSyntax methodSyntax)
            {
                var paramsNode = this.EnqueueNode(methodSyntax.ParameterList);
                var bodyNode = this.EnqueueNode(methodSyntax.Body);
                paramsNode.AddEdge(bodyNode);

                if ((methodSyntax.ReturnType as PredefinedTypeSyntax).Keyword.Text == "void")
                {
                    var implicitEndNode = this.AddNode(methodSyntax.Body.CloseBraceToken);
                    bodyNode.AddEdge(implicitEndNode);

                    // TODO: Add also ReturnFlowNode here
                }

                return Task.CompletedTask;
            }

            private NodeStub AddNode(SyntaxNodeOrToken syntax)
            {
                var node = new NodeStub(syntax);
                this.owner.NodeStubs.Add(node);

                return node;
            }

            private NodeStub EnqueueNode(SyntaxNode syntax)
            {
                var node = this.AddNode(syntax);
                this.owner.ReadyQueue.Enqueue(node);

                return node;
            }

            private void RemoveNode(NodeStub node)
            {
                this.owner.NodeStubs.Remove(node);
            }
        }
    }
}
