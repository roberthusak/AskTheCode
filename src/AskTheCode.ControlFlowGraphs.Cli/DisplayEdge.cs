using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    public class DisplayEdge
    {
        public DisplayEdge(DisplayNode to, string label = null)
        {
            Contract.Requires(to != null);

            this.To = to;
            this.Label = label;
        }

        public DisplayNode To { get; private set; }

        public string Label { get; private set; }
    }
}
