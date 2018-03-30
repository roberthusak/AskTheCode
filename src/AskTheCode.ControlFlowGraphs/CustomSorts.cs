using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public static class CustomSorts
    {
        public static Sort Reference { get; } = Sort.CreateCustom("Reference");
    }
}
