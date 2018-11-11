using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class ReturnFlowNode : FlowNode
    {
        internal ReturnFlowNode(FlowGraph graph, FlowNodeId id, FlowNodeFlags flags, IEnumerable<Expression> returnValues)
            : base(graph, id, flags)
        {
            Contract.Requires(returnValues != null);

            this.ReturnValues = returnValues.ToImmutableArray();
        }

        public IReadOnlyList<Expression> ReturnValues { get; private set; }
    }
}
