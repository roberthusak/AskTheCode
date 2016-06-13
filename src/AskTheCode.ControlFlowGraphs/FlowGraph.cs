using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.Common;
using System.Diagnostics.Contracts;

namespace AskTheCode.ControlFlowGraphs
{
    // TODO: Consider moving the internal mutable collections to the builder in order to save memory
    // TODO: Consider publishing the non-inner nodes (enter, return, call) in separate properties to save CPU during the
    //       path searching (and possibly validation)
    // TODO: Think about the mechanism of how to connect graphs with each other during the path searching (and traverse
    //       these connections as a 'call graph'), while being efficient and extensible (possible custom node types?)
    public class FlowGraph : IIdReferenced<FlowGraphId>, IFreezable<FlowGraph>
    {
        internal FlowGraph(FlowGraphId id, FlowGraphBuilder builder)
        {
            Contract.Requires(id.IsValid);
            Contract.Requires(builder != null);

            this.Id = id;
            this.Builder = builder;

            this.Nodes = this.MutableNodes;
            this.Edges = this.MutableEdges;
        }

        // TODO: Validate? Think about the possible mechanisms of the validation (voluntary/compulsory etc.)
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

        public FlowGraphNode this[FlowGraphNodeId nodeId]
        {
            get { return this.Nodes[nodeId.Value]; }
        }

        public FlowGraphEdge this[FlowGraphEdgeId edgeId]
        {
            get { return this.Edges[edgeId.Value]; }
        }

        public FlowGraphLocalVariable this[FlowGraphLocalVariableId variableId]
        {
            get { return this.LocalVariables[variableId.Value]; }
        }

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

                this.Builder.ReleaseGraph();
                this.Builder = null;
            }

            return new FrozenHandler<FlowGraph>(this);
        }
    }
}
