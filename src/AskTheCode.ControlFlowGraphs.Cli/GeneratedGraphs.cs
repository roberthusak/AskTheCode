using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal struct GeneratedGraphs
    {
        public GeneratedGraphs(FlowGraph flowGraph, DisplayGraph displayGraph)
        {
            this.FlowGraph = flowGraph;
            this.DisplayGraph = displayGraph;
        }

        public FlowGraph FlowGraph { get; private set; }

        public DisplayGraph DisplayGraph { get; private set; }
    }
}
