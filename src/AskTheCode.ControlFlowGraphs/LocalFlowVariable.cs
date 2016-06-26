using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public class LocalFlowVariable : FlowVariable, IIdReferenced<LocalFlowVariableId>
    {
        private readonly string displayName;

        internal LocalFlowVariable(FlowGraph graph, LocalFlowVariableId id, Sort sort, string displayName = null)
            : base(sort)
        {
            Contract.Requires(graph != null);
            Contract.Requires(id.IsValid);

            this.Graph = graph;
            this.Id = id;
            this.displayName = displayName;
        }

        public override string DisplayName
        {
            get { return this.displayName ?? $"var!{this.Graph.Id.Value}!{this.Id.Value}"; }
        }

        public FlowGraph Graph { get; private set; }

        public LocalFlowVariableId Id { get; private set; }
    }
}
