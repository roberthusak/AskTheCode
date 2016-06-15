using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowNode : IIdReferenced<FlowNodeId>, IFreezable<FlowNode>
    {
        internal FlowNode(FlowGraph graph, FlowNodeId id)
        {
            Contract.Requires(graph != null);
            Contract.Requires(id.IsValid);

            this.Graph = graph;
            this.Id = id;
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

        public FlowNodeId Id { get; private set; }

        public FlowGraph Graph { get; private set; }

        public IReadOnlyList<FlowNode> IngoingEdges { get; private set; }

        public IReadOnlyList<FlowNode> OutgoingEdges { get; private set; }

        internal List<FlowNode> MutableIngoingEdges { get; private set; } = new List<FlowNode>();

        internal List<FlowNode> MutableOutgoingEdges { get; private set; } = new List<FlowNode>();

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
