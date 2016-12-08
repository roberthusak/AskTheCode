using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs
{
    public class InnerFlowEdge : FlowEdge, IIdReferenced<InnerFlowEdgeId>
    {
        internal InnerFlowEdge(InnerFlowEdgeId id, FlowNode from, FlowNode to, BoolHandle condition)
            : base(from, to, condition)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(from.Graph == to.Graph);

            this.Graph = from.Graph;
            this.Id = id;
        }

        public FlowGraph Graph { get; private set; }

        public InnerFlowEdgeId Id { get; private set; }
    }
}
