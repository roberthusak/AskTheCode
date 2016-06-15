using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowVariable : Variable
    {
        internal FlowVariable(Sort sort)
            : base(sort)
        {
        }
    }
}
