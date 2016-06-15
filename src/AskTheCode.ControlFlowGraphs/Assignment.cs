using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public struct Assignment
    {
        public Assignment(FlowVariable variable, Expression value)
        {
            Contract.Requires<ArgumentNullException>(variable != null, nameof(variable));
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));
            Contract.Requires<ArgumentException>(value.Sort == variable.Sort, nameof(value));

            this.Variable = variable;
            this.Value = value;
        }

        public FlowVariable Variable { get; private set; }

        public Expression Value { get; private set; }
    }
}
