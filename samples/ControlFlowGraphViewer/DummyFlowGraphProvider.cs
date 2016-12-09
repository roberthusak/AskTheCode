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

        public Task<FlowGraph> GetFlowGraphAsync(ILocation location)
        {
            throw new NotImplementedException();
        }

        public OuterFlowEdge GetCallEdge(CallFlowNode callNode, EnterFlowNode enterNode)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OuterFlowEdge>> GetCallEdgesToAsync(EnterFlowNode enterNode)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<OuterFlowEdge>> GetReturnEdgesToAsync(CallFlowNode callNode)
        {
            throw new NotImplementedException();
        }
    }
}
