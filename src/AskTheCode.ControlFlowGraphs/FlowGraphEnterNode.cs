using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphEnterNode : FlowGraphNode
    {
        internal FlowGraphEnterNode(FlowGraph graph, FlowGraphNodeId id, IEnumerable<FlowGraphVariable> parameters)
            : base(graph, id)
        {
            Contract.Requires(parameters != null);

            this.Parameters = parameters.ToImmutableArray();
        }

        public IReadOnlyList<FlowGraphVariable> Parameters { get; private set; }
    }
}
