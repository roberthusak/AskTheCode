using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs
{
    public enum OuterFlowEdgeKind
    {
        MethodCall,
        Return
    }

    public class OuterFlowEdge : FlowEdge, IIdReferenced<OuterFlowEdgeId>
    {
        internal OuterFlowEdge(OuterFlowEdgeId id, OuterFlowEdgeKind kind, FlowNode from, FlowNode to)
            : base(from, to)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(from.Graph == to.Graph);
            Contract.Requires(kind != OuterFlowEdgeKind.MethodCall || (from is CallFlowNode && to is EnterFlowNode));
            Contract.Requires(kind != OuterFlowEdgeKind.Return || (from is ReturnFlowNode && to is CallFlowNode));

            this.Id = id;
            this.Kind = kind;
        }

        public OuterFlowEdgeId Id { get; private set; }

        public OuterFlowEdgeKind Kind { get; private set; }
    }
}
