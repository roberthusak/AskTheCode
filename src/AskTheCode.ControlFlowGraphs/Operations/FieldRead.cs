using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.TypeSystem;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    public class FieldRead : Operation
    {
        public FieldRead(FlowVariable resultStore, FlowVariable reference, IFieldDefinition field)
        {
            Contract.Requires<ArgumentNullException>(resultStore != null, nameof(resultStore));
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));
            Contract.Requires<ArgumentNullException>(field != null, nameof(field));
            Contract.Requires<ArgumentException>(reference.IsReference, nameof(reference));
            Contract.Requires<ArgumentException>(resultStore.Sort == field.Sort, nameof(resultStore));

            this.ResultStore = resultStore;
            this.Reference = reference;
            this.Field = field;
        }

        public FlowVariable ResultStore { get; }

        public FlowVariable Reference { get; }

        public IFieldDefinition Field { get; }

        public override void Accept(OperationVisitor visitor)
        {
            visitor.VisitFieldRead(this);
        }

        public override TResult Accept<TResult>(OperationVisitor<TResult> visitor)
        {
            return visitor.VisitFieldRead(this);
        }
    }
}
