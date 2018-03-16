using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class InnerFlowNode : FlowNode
    {
        internal InnerFlowNode(FlowGraph graph, FlowNodeId id, IEnumerable<Assignment> assignments)
            : base(graph, id)
        {
            Contract.Requires(assignments != null);

            this.Assignments = assignments.ToImmutableArray();
        }

        public IReadOnlyList<Assignment> Assignments { get; private set; }
    }
}
