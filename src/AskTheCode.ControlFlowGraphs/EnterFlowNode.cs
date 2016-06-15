using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public class EnterFlowNode : FlowNode
    {
        internal EnterFlowNode(FlowGraph graph, FlowNodeId id, IEnumerable<FlowVariable> parameters)
            : base(graph, id)
        {
            Contract.Requires(parameters != null);

            this.Parameters = parameters.ToImmutableArray();
        }

        public IReadOnlyList<FlowVariable> Parameters { get; private set; }
    }
}
