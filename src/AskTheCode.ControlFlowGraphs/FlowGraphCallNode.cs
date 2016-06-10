using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphCallNode : FlowGraphNode
    {
        public IEnumerable<Expression> Arguments
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<FlowGraphVariable> ReturnAssignments
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
