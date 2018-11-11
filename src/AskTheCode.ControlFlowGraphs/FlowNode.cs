using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.Common;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowNode : IIdReferenced<FlowNodeId>, IFreezable<FlowNode>
    {
        internal FlowNode(FlowGraph graph, FlowNodeId id, FlowNodeFlags flags)
        {
            Contract.Requires(graph != null);
            Contract.Requires(id.IsValid);

            this.Graph = graph;
            this.Id = id;
            this.Flags = flags;
            this.IngoingEdges = this.MutableIngoingEdges;
            this.OutgoingEdges = this.MutableOutgoingEdges;
        }

        /// <remarks>
        /// A node of a graph can be frozen only by freezing the graph, not directly.
        /// </remarks>
        public bool CanFreeze
        {
            get { return false; }
        }

        public bool IsFrozen
        {
            get { return (this.MutableIngoingEdges == null); }
        }

        public FlowNodeId Id { get; }

        public FlowGraph Graph { get; }

        public FlowNodeFlags Flags { get; }

        public IReadOnlyList<InnerFlowEdge> IngoingEdges { get; private set; }

        public IReadOnlyList<InnerFlowEdge> OutgoingEdges { get; private set; }

        internal List<InnerFlowEdge> MutableIngoingEdges { get; private set; } = new List<InnerFlowEdge>();

        internal List<InnerFlowEdge> MutableOutgoingEdges { get; private set; } = new List<InnerFlowEdge>();

        public FrozenHandler<FlowNode> Freeze()
        {
            if (!this.IsFrozen)
            {
                this.IngoingEdges = this.MutableIngoingEdges.ToImmutableArray();
                this.MutableIngoingEdges = null;

                this.OutgoingEdges = this.MutableOutgoingEdges.ToImmutableArray();
                this.MutableOutgoingEdges = null;
            }

            return new FrozenHandler<FlowNode>(this);
        }
    }
}
