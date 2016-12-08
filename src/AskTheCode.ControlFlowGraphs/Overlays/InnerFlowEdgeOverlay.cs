using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class InnerFlowEdgeOverlay<T> : OrdinalOverlay<InnerFlowEdgeId, InnerFlowEdge, T>
    {
        public InnerFlowEdgeOverlay(Func<T> defaultValueFactory = null)
            : base(defaultValueFactory)
        {
        }
    }
}
