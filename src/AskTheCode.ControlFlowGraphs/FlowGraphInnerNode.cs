using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphInnerNode : FlowGraphNode
    {
        internal FlowGraphInnerNode(FlowGraph graph, FlowGraphNodeId id, IEnumerable<Assignment> assignments)
            : base(graph, id)
        {
            Contract.Requires(assignments != null);

            this.Assignments = assignments.ToImmutableArray();
        }

        public IReadOnlyList<Assignment> Assignments { get; private set; }
    }
}
