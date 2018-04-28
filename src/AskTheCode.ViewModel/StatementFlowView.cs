using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Heap;

namespace AskTheCode.ViewModel
{
    // TODO: Handle the selection and higlighting
    public class StatementFlowView
    {
        internal StatementFlowView(
            MethodFlowView methodFlowView,
            int index,
            DisplayNodeRecord displayRecord,
            string statement,
            string value,
            string type,
            HeapModelLocation? heapLocation = null,
            MethodFlowView calledMethod = null)
        {
            this.MethodFlowView = methodFlowView;
            this.Index = index;
            this.DisplayRecord = displayRecord;
            this.Statement = statement;
            this.Value = value;
            this.Type = type;
            this.HeapLocation = heapLocation;
            this.CalledMethod = calledMethod;
        }

        public int Index { get; }

        public string Statement { get; }

        public string Value { get; }

        public string Type { get; }

        public HeapModelLocation? HeapLocation { get; }

        internal MethodFlowView MethodFlowView { get; }

        internal DisplayNodeRecord DisplayRecord { get; }

        internal MethodFlowView CalledMethod { get; }
    }
}
