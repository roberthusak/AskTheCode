using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowEdge : IIdReferenced<FlowEdgeId>
    {
        internal FlowEdge(FlowEdgeId id, FlowNode from, FlowNode to, BoolHandle condition)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(from != null);
            Contract.Requires(to != null);
            Contract.Requires(from.Graph == to.Graph);

            this.Graph = from.Graph;
            this.Id = id;
            this.From = from;
            this.To = to;
            this.Condition = condition;
        }

        public FlowGraph Graph { get; private set; }

        public FlowEdgeId Id { get; private set; }

        public FlowNode From { get; private set; }

        public FlowNode To { get; private set; }

        public BoolHandle Condition { get; private set; }
    }
}
