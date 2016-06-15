using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public class ReturnFlowNode : FlowNode
    {
        internal ReturnFlowNode(FlowGraph graph, FlowNodeId id, IEnumerable<Expression> returnValues)
            : base(graph, id)
        {
            Contract.Requires(returnValues != null);

            this.ReturnValues = returnValues.ToImmutableArray();
        }

        public IReadOnlyList<Expression> ReturnValues { get; private set; }
    }
}
