using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public struct Assignment
    {
        public Assignment(FlowGraphVariable variable, Expression value)
        {
            this.Variable = variable;
            this.Value = value;
        }

        public FlowGraphVariable Variable { get; private set; }

        public Expression Value { get; private set; }
    }
}
