using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.TypeSystem;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    public class FieldWrite : Operation
    {
        public FieldWrite(FlowVariable reference, IFieldDefinition field, FlowVariable value)
        {
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));
            Contract.Requires<ArgumentNullException>(field != null, nameof(field));
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));
            Contract.Requires<ArgumentException>(reference.IsReference, nameof(reference));
            Contract.Requires<ArgumentException>(value.Sort == field.Sort, nameof(value));

            this.Reference = reference;
            this.Field = field;
            this.Value = value;
        }

        public FlowVariable Reference { get; }

        public IFieldDefinition Field { get; }

        public FlowVariable Value { get; }

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
