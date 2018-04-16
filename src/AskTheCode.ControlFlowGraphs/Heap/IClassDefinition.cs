using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Heap
{
    public interface IClassDefinition
    {
        AsyncLazy<IEnumerable<IFieldDefinition>> Fields { get; }
    }
}
