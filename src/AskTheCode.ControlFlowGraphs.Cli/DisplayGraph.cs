using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    // TODO: Reflect the rename from FlowGraphCodeMap in the documentation
    public class DisplayGraph : IReadOnlyOverlay<FlowNodeId, FlowNode, DisplayNode>, IFreezable<DisplayGraph>
    {
        private DisplayNodeId.Provider nodeIdProvider = new DisplayNodeId.Provider();
        private List<DisplayNode> mutableNodes = new List<DisplayNode>();

        private OrdinalOverlay<FlowNodeId, FlowNode, DisplayNode> mapFromFlowNodes =
            new OrdinalOverlay<FlowNodeId, FlowNode, DisplayNode>();

        internal DisplayGraph(DocumentId documentId)
        {
            Contract.Requires(documentId != null);

            this.DocumentId = documentId;

            this.Nodes = this.mutableNodes;
        }

        public bool CanFreeze
        {
            get { return true; }
        }

        public bool IsFrozen
        {
            get { return this.mutableNodes == null; }
        }

        public DocumentId DocumentId { get; private set; }

        public IReadOnlyList<DisplayNode> Nodes { get; private set; }

        public DisplayNode this[FlowNode node]
        {
            get { return this.mapFromFlowNodes[node]; }
            internal set { this.mapFromFlowNodes[node] = value; }
        }

        public DisplayNode this[FlowNodeId id]
        {
            get { return this.mapFromFlowNodes[id]; }
            internal set { this.mapFromFlowNodes[id] = value; }
        }

        public FrozenHandler<DisplayGraph> Freeze()
        {
            if (!this.IsFrozen)
            {
                foreach (var node in this.Nodes)
                {
                    node.Freeze();
                }

                this.Nodes = this.mutableNodes.ToImmutableArray();
                this.mutableNodes = null;
            }

            return new FrozenHandler<Cli.DisplayGraph>(this);
        }

        internal DisplayNode AddNode(TextSpan span)
        {
            Contract.Requires<FrozenObjectModificationException>(!this.IsFrozen);

            var nodeId = this.nodeIdProvider.GenerateNewId();
            var node = new DisplayNode(nodeId, span);
            this.mutableNodes.Add(node);
            Contract.Assert(nodeId.Value == this.mutableNodes.IndexOf(node));

            return node;
        }

        internal DisplayEdge AddEdge(DisplayNode from, DisplayNode to, string label = null)
        {
            Contract.Requires<FrozenObjectModificationException>(!this.IsFrozen);

            var edge = new DisplayEdge(to, label);
            from.MutableOutgoingEdges.Add(edge);

            return edge;
        }
    }
}
