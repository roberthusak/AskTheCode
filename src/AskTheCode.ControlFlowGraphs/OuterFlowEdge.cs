using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public enum OuterFlowEdgeKind
    {
        MethodCall,
        Return
    }

    public class OuterFlowEdge : FlowEdge, IIdReferenced<OuterFlowEdgeId>
    {
        private OuterFlowEdge(OuterFlowEdgeId id, OuterFlowEdgeKind kind, FlowNode from, FlowNode to)
            : base(from, to)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(kind != OuterFlowEdgeKind.MethodCall || (from is CallFlowNode && to is EnterFlowNode));
            Contract.Requires(kind != OuterFlowEdgeKind.Return || (from is ReturnFlowNode && to is CallFlowNode));

            this.Id = id;
            this.Kind = kind;
        }

        public OuterFlowEdgeId Id { get; private set; }

        public OuterFlowEdgeKind Kind { get; private set; }

        public static OuterFlowEdge CreateMethodCall(OuterFlowEdgeId id, CallFlowNode callNode, EnterFlowNode enterNode)
        {
            return new OuterFlowEdge(id, OuterFlowEdgeKind.MethodCall, callNode, enterNode);
        }

        public static OuterFlowEdge CreateReturn(OuterFlowEdgeId id, ReturnFlowNode returnNode, CallFlowNode callNode)
        {
            return new OuterFlowEdge(id, OuterFlowEdgeKind.Return, returnNode, callNode);
        }
    }
}
