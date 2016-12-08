using AskTheCode.SmtLibStandard.Handles;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowEdge
    {
        internal FlowEdge(FlowNode from, FlowNode to, BoolHandle condition)
        {
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Requires(condition.Expression != null);

            this.From = from;
            this.To = to;
            this.Condition = condition;
        }

        public FlowNode From { get; private set; }

        public FlowNode To { get; private set; }

        public BoolHandle Condition { get; private set; }
    }
}
