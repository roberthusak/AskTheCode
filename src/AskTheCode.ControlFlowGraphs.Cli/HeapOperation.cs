using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.ControlFlowGraphs.Cli.TypeSystem;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class HeapOperation : SpecialOperation
    {
        public HeapOperation(SpecialOperationKind kind, ReferenceModel reference, FieldDefinition field)
            : base(kind)
        {
            Contract.Requires(IsKindSupported(kind));
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));
            Contract.Requires<ArgumentNullException>(field != null, nameof(field));

            this.Reference = reference;
            this.Field = field;
        }

        public ReferenceModel Reference { get; }

        public FieldDefinition Field { get; }

        public static bool IsKindSupported(SpecialOperationKind kind)
        {
            switch (kind)
            {
                case SpecialOperationKind.FieldRead:
                case SpecialOperationKind.FieldWrite:
                    return true;

                default:
                    return false;
            }
        }
    }
}
