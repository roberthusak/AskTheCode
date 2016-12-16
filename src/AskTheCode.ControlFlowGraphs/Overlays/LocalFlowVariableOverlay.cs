using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class LocalFlowVariableOverlay<T> : OrdinalOverlay<LocalFlowVariableId, LocalFlowVariable, T>
    {
        public LocalFlowVariableOverlay(Func<T> defaultValueFactory = null)
            : base(defaultValueFactory)
        {
        }

        public new LocalFlowVariableOverlay<T> Clone(Func<T, T> valueCloner = null)
        {
            return this.CloneImpl(new LocalFlowVariableOverlay<T>(), valueCloner);
        }
    }
}
