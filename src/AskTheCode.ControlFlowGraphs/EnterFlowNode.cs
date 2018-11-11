using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class EnterFlowNode : FlowNode
    {
        internal EnterFlowNode(FlowGraph graph, FlowNodeId id, FlowNodeFlags flags, IEnumerable<FlowVariable> parameters)
            : base(graph, id, flags)
        {
            Contract.Requires(parameters != null);

            this.Parameters = parameters.ToImmutableArray();
        }

        public IReadOnlyList<FlowVariable> Parameters { get; private set; }
    }
}
