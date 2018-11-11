using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.ControlFlowGraphs.Operations;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class InnerFlowNode : FlowNode
    {
        internal InnerFlowNode(FlowGraph graph, FlowNodeId id, FlowNodeFlags flags, IEnumerable<Operation> operations)
            : base(graph, id, flags)
        {
            Contract.Requires(operations != null);

            this.Operations = operations.ToImmutableArray();
        }

        public IReadOnlyList<Operation> Operations { get; private set; }
    }
}
