using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphInnerNode : FlowGraphNode
    {
        public IEnumerable<Assignment> Assignments
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
