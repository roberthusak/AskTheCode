using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class CallFlowNode : FlowNode
    {
        internal CallFlowNode(
            FlowGraph graph,
            FlowNodeId id,
            ILocation location,
            IEnumerable<Expression> arguments,
            IEnumerable<FlowVariable> returnAssignments)
            : base(graph, id)
        {
            Contract.Requires(location != null);
            Contract.Requires(arguments != null);
            Contract.Requires(returnAssignments != null);

            this.Location = location;
            this.Arguments = arguments.ToImmutableArray();
            this.ReturnAssignments = returnAssignments.ToImmutableArray();
        }

        public ILocation Location { get; private set; }

        public IReadOnlyList<Expression> Arguments { get; private set; }

        public IReadOnlyList<FlowVariable> ReturnAssignments { get; private set; }
    }
}
