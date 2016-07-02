using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public interface IFlowGraphProvider
    {
        FlowGraph this[FlowGraphId graphId] { get; }

        FlowGraph GetFlowGraphAsync(ILocation location);
    }
}
