using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    /// <summary>
    /// Represents a heap operation concerning a field.
    /// </summary>
    public abstract class FieldOperation : Operation
    {
        protected FieldOperation(FlowVariable reference, IFieldDefinition field)
        {
            Contract.Requires(reference != null, nameof(reference));
            Contract.Requires(field != null, nameof(field));
            Contract.Requires(reference.IsReference, nameof(reference));

            this.Reference = reference;
            this.Field = field;
        }

        public FlowVariable Reference { get; }

        public IFieldDefinition Field { get; }
    }
}
