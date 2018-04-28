using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli;

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
            string type)
        {
            this.MethodFlowView = methodFlowView;
            this.Index = index;
            this.DisplayRecord = displayRecord;
            this.Statement = statement;
            this.Value = value;
            this.Type = type;
        }

        public int Index { get; }

        public string Statement { get; private set; }

        public string Value { get; private set; }

        public string Type { get; private set; }

        internal MethodFlowView MethodFlowView { get; private set; }

        internal DisplayNodeRecord DisplayRecord { get; private set; }
    }
}
