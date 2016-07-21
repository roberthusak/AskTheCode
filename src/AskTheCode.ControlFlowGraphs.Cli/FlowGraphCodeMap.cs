using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    public class FlowGraphCodeMap : IReadOnlyOverlay<FlowNodeId, FlowNode, CodeMapRecord>
    {
        private CodeMapRecord[] values;

        internal FlowGraphCodeMap(int nodeCount, DocumentId documentId)
        {
            Contract.Requires(nodeCount >= 0);
            Contract.Requires(documentId != null);

            this.DocumentId = documentId;

            this.values = new CodeMapRecord[nodeCount];
        }

        public DocumentId DocumentId { get; private set; }

        public CodeMapRecord this[FlowNode node]
        {
            get { return this[node.Id]; }
            internal set { this[node.Id] = value; }
        }

        public CodeMapRecord this[FlowNodeId id]
        {
            get { return this.values[id.Value]; }
            internal set { this.values[id.Value] = value; }
        }
    }
}
