using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class ThrowExceptionFlowNode : FlowNode
    {
        internal ThrowExceptionFlowNode(
            FlowGraph graph,
            FlowNodeId id,
            FlowNodeFlags flags,
            IRoutineLocation constructorLocation,
            IEnumerable<Expression> arguments)
            : base(graph, id, flags)
        {
            Contract.Requires(constructorLocation != null);
            Contract.Requires(arguments != null);

            this.ConstructorLocation = constructorLocation;
            this.Arguments = arguments.ToImmutableArray();
        }

        public IRoutineLocation ConstructorLocation { get; private set; }

        public IReadOnlyList<Expression> Arguments { get; private set; }
    }
}
