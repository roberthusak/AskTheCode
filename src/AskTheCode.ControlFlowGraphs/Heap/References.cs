using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Heap
{
    public static class References
    {
        public static Sort Sort { get; } = Sort.CreateCustom("Reference");

        public static SpecialFlowVariable Null { get; } = new SpecialFlowVariable("null", Sort);
    }
}
