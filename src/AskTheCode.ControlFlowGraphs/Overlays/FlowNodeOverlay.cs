using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class FlowNodeOverlay<T> : OrdinalOverlay<FlowNodeId, FlowNode, T>
    {
        public FlowNodeOverlay(Func<T> defaultValueFactory = null)
            : base(defaultValueFactory)
        {
        }

        public new FlowNodeOverlay<T> Clone(Func<T, T> valueCloner = null)
        {
            return this.CloneImpl(new FlowNodeOverlay<T>(), valueCloner);
        }
    }
}
