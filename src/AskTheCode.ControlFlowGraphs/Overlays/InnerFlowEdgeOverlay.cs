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

        public new InnerFlowEdgeOverlay<T> Clone(Func<T, T> valueCloner = null)
        {
            return this.CloneImpl(new InnerFlowEdgeOverlay<T>(), valueCloner);
        }
    }
}
