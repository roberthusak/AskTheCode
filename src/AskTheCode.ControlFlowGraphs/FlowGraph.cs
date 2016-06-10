using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraph : IIdReferenced<FlowGraphId>, IFreezable<FlowGraph>
    {
        internal FlowGraph(FlowGraphId id, FlowGraphBuilder builder)
        {
            this.Id = id;
            this.Builder = builder;

            this.Nodes = this.MutableNodes;
            this.Edges = this.MutableEdges;
        }

        // TODO: Validate?
        public bool CanFreeze
        {
            get { return true; }
        }

        public bool IsFrozen
        {
            get { return (this.Builder == null); }
        }

        public FlowGraphId Id { get; private set; }

        public IReadOnlyList<FlowGraphNode> Nodes { get; private set; }

        public IReadOnlyList<FlowGraphEdge> Edges { get; private set; }

        public IReadOnlyList<FlowGraphLocalVariable> LocalVariables { get; private set; }

        internal FlowGraphBuilder Builder { get; private set; }

        internal List<FlowGraphNode> MutableNodes { get; private set; } = new List<FlowGraphNode>();

        internal List<FlowGraphEdge> MutableEdges { get; private set; } = new List<FlowGraphEdge>();

        internal List<FlowGraphLocalVariable> MutableLocalVariables { get; private set; }
            = new List<FlowGraphLocalVariable>();

        public FrozenHandler<FlowGraph> Freeze()
        {
            if (!this.IsFrozen)
            {
                foreach (var node in this.Nodes)
                {
                    node.Freeze();
                }

                this.Nodes = this.MutableNodes.ToImmutableArray();
                this.MutableNodes = null;

                this.Edges = this.MutableEdges.ToImmutableArray();
                this.MutableEdges = null;

                this.LocalVariables = this.MutableLocalVariables.ToImmutableArray();
                this.MutableLocalVariables = null;

                this.Builder.Graph = null;
                this.Builder = null;
            }

            return new FrozenHandler<FlowGraph>(this);
        }
    }
}
