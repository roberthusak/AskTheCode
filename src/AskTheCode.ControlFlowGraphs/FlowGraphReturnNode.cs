using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphReturnNode : FlowGraphNode
    {
        internal FlowGraphReturnNode(FlowGraph graph, FlowGraphNodeId id, IEnumerable<Expression> returnValues)
            : base(graph, id)
        {
            Contract.Requires(returnValues != null);

            this.ReturnValues = returnValues.ToImmutableArray();
        }

        public IReadOnlyList<Expression> ReturnValues { get; private set; }
    }
}
