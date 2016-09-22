using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    public class DisplayNodeRecord
    {
        // TODO: Consider storing also the particular type (or store it instead of the factory and load it lazily)
        public DisplayNodeRecord(
            FlowNode flowNode,
            TextSpan span,
            int firstVariableIndex = -1,
            ITypeModelFactory modelFactory = null)
        {
            this.FlowNode = flowNode;
            this.Span = span;
            this.FirstVariableIndex = firstVariableIndex;
            this.ModelFactory = modelFactory;
        }

        public FlowNode FlowNode { get; private set; }

        public TextSpan Span { get; private set; }

        public int FirstVariableIndex { get; private set; }

        public ITypeModelFactory ModelFactory { get; private set; }

    }
}
