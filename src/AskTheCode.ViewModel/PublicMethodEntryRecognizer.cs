using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.PathExploration;

namespace AskTheCode.ViewModel
{
    // TODO: Consider moving to another project (e.g. PathExploration.Cli?) - it is more of an logic than just view
    public class PublicMethodEntryRecognizer : IEntryPointRecognizer
    {
        public bool IsFinalNode(FlowNode node)
        {
            if (!(node is EnterFlowNode))
            {
                return false;
            }
            else
            {
                // TODO: Remove this temporal hack! (IFlowGraphProvider must be somehow propagated instead)
                return (node.Graph.Id.Value > 0);
            }
        }
    }
}
