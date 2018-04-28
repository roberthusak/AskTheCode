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
            ITypeSymbol type = null,
            string variableName = null)
        {
            this.FlowNode = flowNode;
            this.Span = span;
            this.FirstVariableIndex = firstVariableIndex;
            this.Type = type;
            this.VariableName = variableName;
        }

        public FlowNode FlowNode { get; }

        public TextSpan Span { get; }

        public int FirstVariableIndex { get; }

        public ITypeSymbol Type { get; }

        public string VariableName { get; }
    }
}
