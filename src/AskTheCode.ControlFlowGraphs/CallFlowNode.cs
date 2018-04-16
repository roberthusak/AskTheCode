using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class CallFlowNode : FlowNode
    {
        internal CallFlowNode(
            FlowGraph graph,
            FlowNodeId id,
            IRoutineLocation location,
            IEnumerable<Expression> arguments,
            IEnumerable<FlowVariable> returnAssignments,
            bool isAllocationCall)
            : base(graph, id)
        {
            Contract.Requires(location != null);
            Contract.Requires(arguments != null);
            Contract.Requires(returnAssignments != null);
            Contract.Requires(
                !isAllocationCall || VerifyConstructorUsage(location, returnAssignments),
                nameof(isAllocationCall));

            this.Location = location;
            this.Arguments = arguments.ToImmutableArray();
            this.ReturnAssignments = returnAssignments.ToImmutableArray();
            this.IsConstructorCall = isAllocationCall;
        }

        public IRoutineLocation Location { get; }

        public IReadOnlyList<Expression> Arguments { get; }

        public IReadOnlyList<FlowVariable> ReturnAssignments { get; }

        public bool IsConstructorCall { get; }

        private static bool VerifyConstructorUsage(
            IRoutineLocation location,
            IEnumerable<FlowVariable> returnAssignments)
        {
            return location.IsConstructor
                && returnAssignments.Count() == 1 && returnAssignments.Single().Sort == References.Sort;
        }
    }
}
