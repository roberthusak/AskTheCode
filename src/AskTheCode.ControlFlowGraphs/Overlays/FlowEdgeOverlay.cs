using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class FlowEdgeOverlay<T> : OrdinalOverlay<FlowEdgeId, FlowEdge, T>
    {
        public FlowEdgeOverlay(Func<T> defaultValueFactory = null)
            : base(defaultValueFactory)
        {
        }
    }
}
