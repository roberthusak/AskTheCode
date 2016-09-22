using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs
{
    public interface IFlowGraphProvider
    {
        FlowGraph this[FlowGraphId graphId] { get; }

        Task<FlowGraph> GetFlowGraphAsync(ILocation location);
    }
}
