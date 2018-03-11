using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class InnerFlowEdge : FlowEdge, IIdReferenced<InnerFlowEdgeId>
    {
        internal InnerFlowEdge(InnerFlowEdgeId id, FlowNode from, FlowNode to, BoolHandle condition)
            : base(from, to)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(from.Graph == to.Graph);
            Contract.Requires(condition.Expression != null);

            this.Graph = from.Graph;
            this.Id = id;
            this.Condition = condition;
        }

        public FlowGraph Graph { get; private set; }

        public InnerFlowEdgeId Id { get; private set; }

        public BoolHandle Condition { get; private set; }
    }
}
