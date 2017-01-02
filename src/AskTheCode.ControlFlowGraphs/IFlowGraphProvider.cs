using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs
{
    // TODO: Enable tasks cancellation
    public interface IFlowGraphProvider
    {
        FlowGraph this[FlowGraphId graphId] { get; }

        ILocation GetLocation(FlowGraphId graphId);

        Task<FlowGraph> GetFlowGraphAsync(ILocation location);

        OuterFlowEdge GetCallEdge(CallFlowNode callNode, EnterFlowNode enterNode);

        Task<IReadOnlyList<OuterFlowEdge>> GetCallEdgesToAsync(EnterFlowNode enterNode);

        Task<IReadOnlyList<OuterFlowEdge>> GetReturnEdgesToAsync(CallFlowNode callNode);

        // TODO: Consider implementing also these (for possible forward symbolic execution)
        ////OuterFlowEdge GetReturnEdge(ReturnFlowNode returnNode, CallFlowNode callNode);
        ////Task<OuterFlowEdge> GetCallEdgeFromAsync(CallFlowNode enterNode);
        ////Task<IReadOnlyList<OuterFlowEdge>> GetReturnEdgesFromAsync(ReturnFlowNode returnNode);
    }
}
