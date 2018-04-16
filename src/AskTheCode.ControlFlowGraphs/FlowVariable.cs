using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowVariable : Variable
    {
        internal FlowVariable(Sort sort)
            : base(sort)
        {
        }

        public bool IsReference => this.Sort == References.Sort;
    }
}
