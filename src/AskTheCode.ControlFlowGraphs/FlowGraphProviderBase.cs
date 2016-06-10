using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowGraphProviderBase
    {
        public abstract FlowGraph GetFlowGraphAsync(ILocation location);

        public abstract FlowGraph this[FlowGraphId graphId] { get; }
    }
}
