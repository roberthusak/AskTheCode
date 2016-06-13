using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphCallNode : FlowGraphNode
    {
        internal FlowGraphCallNode(
            FlowGraph graph,
            FlowGraphNodeId id,
            IEnumerable<Expression> arguments,
            IEnumerable<FlowGraphVariable> returnAssignments)
            : base(graph, id)
        {
            Contract.Requires(arguments != null);
            Contract.Requires(returnAssignments != null);

            this.Arguments = arguments.ToImmutableArray();
            this.ReturnAssignments = returnAssignments.ToImmutableArray();
        }

        public IReadOnlyList<Expression> Arguments { get; private set; }

        public IReadOnlyList<FlowGraphVariable> ReturnAssignments { get; private set; }
    }
}
