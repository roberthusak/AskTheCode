using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class NodeStub
    {
        public NodeStub(SyntaxNodeOrToken syntax)
        {
            this.Syntax = syntax;
        }

        public List<EdgeStub> OutgoingEdges { get; } = new List<EdgeStub>();

        // TODO: Optimize the type if necessary (make 2 fields?)
        public SyntaxNodeOrToken Syntax { get; set; }

        public FlowNode Node { get; set; }

        public Expression Value { get; set; }

        public Task PendingTask { get; set; }

        public EdgeStub AddEdge(NodeStub to, Expression valueCondition = null)
        {
            var edge = new EdgeStub(to, valueCondition);
            this.OutgoingEdges.Add(edge);

            return edge;
        }

        // TODO: Add proper hashing
    }
}
