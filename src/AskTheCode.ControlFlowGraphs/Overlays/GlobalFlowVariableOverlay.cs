using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Overlays
{
    public class GlobalFlowVariableOverlay<T> : OrdinalOverlay<GlobalFlowVariableId, GlobalFlowVariable, T>
    {
        public GlobalFlowVariableOverlay(Func<T> defaultValueFactory = null)
            : base(defaultValueFactory)
        {
        }

        public new GlobalFlowVariableOverlay<T> Clone(Func<T, T> valueCloner = null)
        {
            return this.CloneImpl(new GlobalFlowVariableOverlay<T>(), valueCloner);
        }
    }
}
