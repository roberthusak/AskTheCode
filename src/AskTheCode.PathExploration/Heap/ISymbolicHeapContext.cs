using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.PathExploration.Heap
{
    public interface ISymbolicHeapContext
    {
        NamedVariable GetNamedVariable(VersionedVariable variable);

        void AddAssertion(BoolHandle boolHandle);
    }
}
