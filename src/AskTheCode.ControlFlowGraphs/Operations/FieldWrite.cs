using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    public class FieldWrite : FieldOperation
    {
        public FieldWrite(FlowVariable reference, IFieldDefinition field, Expression value)
            : base(reference, field)
        {
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));
            Contract.Requires<ArgumentNullException>(field != null, nameof(field));
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));
            Contract.Requires<ArgumentException>(reference.IsReference, nameof(reference));
            Contract.Requires<ArgumentException>(value.Sort == field.Sort, nameof(value));

            this.Value = value;
        }

        public Expression Value { get; }

        public override void Accept(OperationVisitor visitor)
        {
            visitor.VisitFieldWrite(this);
        }

        public override TResult Accept<TResult>(OperationVisitor<TResult> visitor)
        {
            return visitor.VisitFieldWrite(this);
        }
    }
}
