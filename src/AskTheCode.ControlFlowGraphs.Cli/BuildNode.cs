using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuildNode
    {
        public BuildNode(SyntaxNodeOrToken syntax)
        {
            this.Syntax = syntax;
        }

        public List<BuildEdge> OutgoingEdges { get; } = new List<BuildEdge>();

        // TODO: Optimize the type if necessary (make 2 fields?)
        public SyntaxNodeOrToken Syntax { get; set; }

        // TODO: Set once semantic? Or remove completely and care about only in the second phase?
        public FlowNode FlowNode { get; set; }

        public Expression Value { get; set; }

        public Task PendingTask { get; set; }

        public BuildEdge AddEdge(BuildNode to, Expression valueCondition = null)
        {
            var edge = new BuildEdge(to, valueCondition);
            this.OutgoingEdges.Add(edge);

            return edge;
        }

        public void AddEdge(BuildEdge edge)
        {
            this.OutgoingEdges.Add(edge);
        }

        // TODO: Add proper hashing
    }
}
