using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    /// <summary>
    /// Represents an assignment of an expression to a variable.
    /// </summary>
    public class Assignment : Operation
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

        public bool IsReference => this.Variable.IsReference;

        public override void Accept(OperationVisitor visitor)
        {
            visitor.VisitAssignment(this);
        }

        public override TResult Accept<TResult>(OperationVisitor<TResult> visitor)
        {
            return visitor.VisitAssignment(this);
        }
    }
}
