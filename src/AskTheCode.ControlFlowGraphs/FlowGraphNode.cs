using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public abstract class FlowGraphNode : IIdReferenced<FlowGraphNodeId>, IFreezable<FlowGraphNode>
    {
        internal FlowGraphNode(FlowGraph graph, FlowGraphNodeId id)
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

        public FlowGraphNodeId Id { get; private set; }

        public FlowGraph Graph { get; private set; }

        public IReadOnlyList<FlowGraphNode> IngoingEdges { get; private set; }

        public IReadOnlyList<FlowGraphNode> OutgoingEdges { get; private set; }

        internal List<FlowGraphNode> MutableIngoingEdges { get; private set; } = new List<FlowGraphNode>();

        internal List<FlowGraphNode> MutableOutgoingEdges { get; private set; } = new List<FlowGraphNode>();

        public FrozenHandler<FlowGraphNode> Freeze()
        {
            if (!this.IsFrozen)
            {
                this.IngoingEdges = this.MutableIngoingEdges.ToImmutableArray();
                this.MutableIngoingEdges = null;

                this.OutgoingEdges = this.MutableOutgoingEdges.ToImmutableArray();
                this.MutableOutgoingEdges = null;
            }

            return new FrozenHandler<FlowGraphNode>(this);
        }
    }
}
