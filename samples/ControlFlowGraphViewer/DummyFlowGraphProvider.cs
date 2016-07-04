using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;

namespace ControlFlowGraphViewer
{
    public class DummyFlowGraphProvider : IFlowGraphProvider
    {
        public FlowGraph this[FlowGraphId graphId]
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public FlowGraph GetFlowGraphAsync(ILocation location)
        {
            throw new NotImplementedException();
        }
    }
}
