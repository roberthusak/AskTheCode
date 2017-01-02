using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    public class DisplayNodeRecord
    {
        public DisplayNodeRecord(
            FlowNode flowNode,
            TextSpan span,
            int firstVariableIndex = -1,
            ITypeSymbol type = null)
        {
            this.FlowNode = flowNode;
            this.Span = span;
            this.FirstVariableIndex = firstVariableIndex;
            this.Type = type;
        }

        public FlowNode FlowNode { get; private set; }

        public TextSpan Span { get; private set; }

        public int FirstVariableIndex { get; private set; }

        public ITypeSymbol Type { get; private set; }
    }
}
