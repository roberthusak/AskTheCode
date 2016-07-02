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
    }
}
