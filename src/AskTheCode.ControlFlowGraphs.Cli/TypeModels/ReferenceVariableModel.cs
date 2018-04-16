using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class ReferenceVariableModel : ReferenceModel
    {
        private readonly Variable variable;

        internal ReferenceVariableModel(ReferenceModelFactory factory, Variable variable)
            : base(factory)
        {
            Contract.Requires(variable != References.Null);

            this.variable = variable;
        }

        public override IReadOnlyList<Variable> AssignmentLeft => this.variable.ToSingular();

        public override IReadOnlyList<Expression> AssignmentRight => this.variable.ToSingular();

        public override bool IsLValue => true;
    }
}
