using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public class BorderEntryPointRecognizer : IEntryPointRecognizer
    {
        public IFlowGraphProvider FlowGraphProvider { get; set; }

        public bool IsFinalNode(FlowNode node)
        {
            return (node is EnterFlowNode);
        }
    }
}
