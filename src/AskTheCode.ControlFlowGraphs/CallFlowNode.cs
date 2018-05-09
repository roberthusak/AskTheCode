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
    public enum CallKind
    {
        Static,
        Instance,
        ObjectCreation
    }

    public class CallFlowNode : FlowNode
    {
        internal CallFlowNode(
            FlowGraph graph,
            FlowNodeId id,
            IRoutineLocation location,
            IEnumerable<Expression> arguments,
            IEnumerable<FlowVariable> returnAssignments,
            CallKind kind)
            : base(graph, id)
        {
            Contract.Requires(location != null);
            Contract.Requires(arguments != null);
            Contract.Requires(returnAssignments != null);
            Contract.Requires(
                kind != CallKind.ObjectCreation || VerifyConstructorUsage(location, returnAssignments),
                nameof(kind));

            this.Location = location;
            this.Arguments = arguments.ToImmutableArray();
            this.ReturnAssignments = returnAssignments.ToImmutableArray();
            this.Kind = kind;
        }

        public IRoutineLocation Location { get; }

        public IReadOnlyList<Expression> Arguments { get; }

        public IReadOnlyList<FlowVariable> ReturnAssignments { get; }

        public CallKind Kind { get; }

        public bool IsObjectCreation => this.Kind == CallKind.ObjectCreation;

        public bool IsInstanceCall => this.Kind == CallKind.Instance;

        private static bool VerifyConstructorUsage(
            IRoutineLocation location,
            IEnumerable<FlowVariable> returnAssignments)
        {
            return location.IsConstructor
                && returnAssignments.Count() == 1 && returnAssignments.Single().Sort == References.Sort;
        }
    }
}
