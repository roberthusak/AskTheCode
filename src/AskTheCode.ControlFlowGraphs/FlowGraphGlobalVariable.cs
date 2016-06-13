using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs
{
    public class FlowGraphGlobalVariable : FlowGraphVariable, IIdReferenced<FlowGraphGlobalVariableId>
    {
        private readonly string displayName;

        public FlowGraphGlobalVariable(FlowGraphGlobalVariableId id, Sort sort, string displayName = null)
            : base(sort)
        {
            Contract.Requires(id.IsValid);

            this.Id = id;
            this.displayName = displayName;
        }

        public override string DisplayName
        {
            get { return this.displayName ?? $"var<{this.Sort.Name}>!global!{this.Id.Value}"; }
        }

        public FlowGraphGlobalVariableId Id { get; private set; }
    }
}
