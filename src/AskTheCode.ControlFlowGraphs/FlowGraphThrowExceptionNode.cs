using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphThrowExceptionNode : FlowGraphNode
    {
        internal FlowGraphThrowExceptionNode(
            FlowGraph graph,
            FlowGraphNodeId id,
            ILocation constructorLocation,
            IEnumerable<Expression> arguments)
            : base(graph, id)
        {
            Contract.Requires(constructorLocation != null);
            Contract.Requires(arguments != null);

            this.ConstructorLocation = constructorLocation;
            this.Arguments = arguments.ToImmutableArray();
        }

        public ILocation ConstructorLocation { get; private set; }

        public IReadOnlyList<Expression> Arguments { get; private set; }
    }
}
