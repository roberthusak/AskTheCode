using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class OuterFlowEdge : FlowEdge, IIdReferenced<OuterFlowEdgeId>
    {
        internal OuterFlowEdge(OuterFlowEdgeId id, FlowNode from, FlowNode to, BoolHandle condition)
            : base(from, to, condition)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(from.Graph == to.Graph);

            this.Id = id;
        }

        public OuterFlowEdgeId Id { get; private set; }
    }
}
