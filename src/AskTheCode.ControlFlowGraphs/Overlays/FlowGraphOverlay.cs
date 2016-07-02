using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class FlowGraphOverlay<T> : OrdinalOverlay<FlowGraphId, FlowGraph, T>
    {
        public FlowGraphOverlay(Func<T> defaultValueFactory = null)
            : base(defaultValueFactory)
        {
        }
    }
}
