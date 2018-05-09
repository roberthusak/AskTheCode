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
    /// <summary>
    /// Models a reference variable.
    /// </summary>
    public class ReferenceVariableModel : ReferenceModel
    {
        internal ReferenceVariableModel(ReferenceModelFactory factory, Variable variable)
            : base(factory)
        {
            Contract.Requires(variable != References.Null);

            this.Variable = variable;
        }

        public Variable Variable { get; }

        public override IReadOnlyList<Variable> AssignmentLeft => this.Variable.ToSingular();

        public override IReadOnlyList<Expression> AssignmentRight => this.Variable.ToSingular();

        public override bool IsLValue => true;

        public override string ToString() => this.Variable.DisplayName;
    }
}
