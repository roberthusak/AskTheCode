using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal enum SpecialOperationKind
    {
        Enter,
        Return,
        MethodCall,
        ExceptionThrow,
        Assertion,

        FieldRead,
        FieldWrite,
    }

    internal abstract class SpecialOperation
    {
        public SpecialOperation(SpecialOperationKind kind)
        {
            this.Kind = kind;
        }

        public SpecialOperationKind Kind { get; }
    }
}
