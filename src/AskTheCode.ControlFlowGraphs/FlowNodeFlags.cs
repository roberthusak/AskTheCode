using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs
{
    [Flags]
    public enum FlowNodeFlags
    {
        None = 0x00,
        LoopCondition = 0x01,
        LoopBody = 0x02
    }
}
