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
    public class DisplayNode : IIdReferenced<DisplayNodeId>, IFreezable<DisplayNode>
    {
        internal DisplayNode(DisplayNodeId id, TextSpan span)
        {
            this.Id = id;
            this.Span = span;

            this.Records = this.MutableRecords;
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
            get { return this.MutableOutgoingEdges == null; }
        }

        public DisplayNodeId Id { get; private set; }

        public TextSpan Span { get; private set; }

        public IReadOnlyList<DisplayNodeRecord> Records { get; private set; }

        public IReadOnlyList<DisplayEdge> OutgoingEdges { get; private set; }

        internal List<DisplayNodeRecord> MutableRecords { get; private set; } = new List<DisplayNodeRecord>();

        internal List<DisplayEdge> MutableOutgoingEdges { get; private set; } = new List<DisplayEdge>();

        public FrozenHandler<DisplayNode> Freeze()
        {
            if (!this.IsFrozen)
            {
                this.Records = this.MutableRecords.ToImmutableArray();
                this.MutableRecords = null;

                this.OutgoingEdges = this.MutableOutgoingEdges.ToImmutableArray();
                this.MutableOutgoingEdges = null;
            }

            return new FrozenHandler<DisplayNode>(this);
        }

        internal void AddRecord(DisplayNodeRecord record)
        {
            Contract.Requires<FrozenObjectModificationException>(!this.IsFrozen);

            this.MutableRecords.Add(record);
        }
    }
}
