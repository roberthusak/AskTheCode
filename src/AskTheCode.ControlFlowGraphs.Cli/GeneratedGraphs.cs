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
            this.Location = null;
            this.FlowGraph = flowGraph;
            this.DisplayGraph = displayGraph;
        }

        /// <remarks>
        /// Expected to be populated by <see cref="CSharpFlowGraphProvider"/> after the translation from BuildGraph.
        /// </remarks>
        public MethodLocation Location { get; set; }

        public FlowGraph FlowGraph { get; private set; }

        public DisplayGraph DisplayGraph { get; private set; }
    }
}
