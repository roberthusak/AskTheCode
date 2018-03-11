using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs
{
    public class GlobalFlowVariable : FlowVariable, IIdReferenced<GlobalFlowVariableId>
    {
        private readonly string displayName;

        public GlobalFlowVariable(GlobalFlowVariableId id, Sort sort, string displayName = null)
            : base(sort)
        {
            Contract.Requires(id.IsValid);

            this.Id = id;
            this.displayName = displayName;
        }

        public override string DisplayName
        {
            get { return this.displayName ?? $"var!global!{this.Id.Value}"; }
        }

        public GlobalFlowVariableId Id { get; private set; }
    }
}
