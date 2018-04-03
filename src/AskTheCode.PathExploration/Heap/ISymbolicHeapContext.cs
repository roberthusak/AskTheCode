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
        VersionedVariable GetVersioned(FlowVariable variable);

        NamedVariable GetNamedVariable(VersionedVariable variable);

        NamedVariable CreateVariable(Sort sort, string name = null);

        void AddAssertion(BoolHandle boolHandle);
    }
}
