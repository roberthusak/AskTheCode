using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;

namespace AskTheCode.ViewModel
{
    public class VariableReplayView
    {
        internal VariableReplayView(string variable, string value, string type, HeapModelLocation? heapLocation)
        {
            this.Variable = variable;
            this.Value = value;
            this.Type = type;
            this.HeapLocation = heapLocation;
        }

        public string Variable { get; }

        public string Value { get; internal set; }

        public string Type { get; internal set; }

        public HeapModelLocation? HeapLocation { get; internal set; }
    }
}
