using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    // TODO: Think out how to map different assignments from one flow node to different spans
    public struct CodeMapRecord
    {
        internal CodeMapRecord(TextSpan span, ITypeSymbol typeSymbol = null, FlowNodeId? parentNodeId = null)
        {
            this.Span = span;
            this.TypeSymbol = typeSymbol;
            this.ParentNodeId = parentNodeId;
        }

        public TextSpan Span { get; private set; }

        public ITypeSymbol TypeSymbol { get; private set; }

        public FlowNodeId? ParentNodeId { get; private set; }
    }
}
