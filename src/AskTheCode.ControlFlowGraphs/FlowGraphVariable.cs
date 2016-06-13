using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowGraphVariable : Variable
    {
        internal FlowGraphVariable(Sort sort)
            : base(sort)
        {
        }
    }
}
