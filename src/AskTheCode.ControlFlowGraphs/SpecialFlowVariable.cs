using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    /// <summary>
    /// Represents a helper variable used in CFGs.
    /// </summary>
    public class SpecialFlowVariable : FlowVariable
    {
        public SpecialFlowVariable(string name, Sort sort)
            : base(sort)
        {
            this.DisplayName = name;
        }

        public override string DisplayName { get; }
    }
}
