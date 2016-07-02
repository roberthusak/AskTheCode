using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public class Path
    {
        public Path(ImmutableArray<Path> preceeding, int depth, FlowNode node, ImmutableArray<FlowEdge> leadingEdges)
        {
            this.Preceeding = preceeding;
            this.Depth = depth;
            this.Node = node;
            this.LeadingEdges = leadingEdges;
        }

        public ImmutableArray<Path> Preceeding { get; private set; }

        public int Depth { get; private set; }

        public FlowNode Node { get; private set; }

        public ImmutableArray<FlowEdge> LeadingEdges { get; private set; }

        public bool IsRoot
        {
            get { return this.Depth == 0; }
        }
    }
}
