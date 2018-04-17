using System;
using System.Collections.Generic;
using System.Collections.Immutable;
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
        public HeapOperation(
            SpecialOperationKind kind,
            ReferenceModel reference,
            ImmutableArray<FieldDefinition> fields)
            : base(kind)
        {
            Contract.Requires(IsKindSupported(kind));
            Contract.Requires<ArgumentNullException>(reference != null, nameof(reference));
            Contract.Requires<ArgumentNullException>(fields != null, nameof(fields));

            this.Reference = reference;
            this.Fields = fields;
        }

        public ReferenceModel Reference { get; }

        public ImmutableArray<FieldDefinition> Fields { get; }

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
