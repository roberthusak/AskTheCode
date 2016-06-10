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
        internal FlowGraphNode(FlowGraph graph, FlowGraphNodeId id, IEnumerable<Assignment> assignments)
        {
            Contract.Requires(graph != null);
            Contract.Requires(!id.Equals(default(FlowGraphNodeId)));
            Contract.Requires(assignments != null);

            this.Graph = graph;
            this.Id = id;
            this.Assignments = assignments.ToImmutableArray();
        }

        public bool CanFreeze
        {
            get { return true; }
        }

        public bool IsFrozen
        {
            get { return (this.MutableIngoingEdges == null); }
        }

        public FlowGraphNodeId Id { get; private set; }

        public FlowGraph Graph { get; private set; }

        public IReadOnlyList<Assignment> Assignments { get; private set; }

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
