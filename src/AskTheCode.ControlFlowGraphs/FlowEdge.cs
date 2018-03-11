using AskTheCode.SmtLibStandard.Handles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowEdge
    {
        internal FlowEdge(FlowNode from, FlowNode to)
        {
            Contract.Requires(from != null);
            Contract.Requires(to != null);

            this.From = from;
            this.To = to;
        }

        public FlowNode From { get; private set; }

        public FlowNode To { get; private set; }
    }
}
