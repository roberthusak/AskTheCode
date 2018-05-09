using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    /// <summary>
    /// Models a null reference.
    /// </summary>
    public class NullReferenceModel : ReferenceModel, IValueModel
    {
        internal NullReferenceModel(ReferenceModelFactory factory)
            : base(factory)
        {
        }

        public override IReadOnlyList<Variable> AssignmentLeft => throw new InvalidAssignmentModelException();

        public override IReadOnlyList<Expression> AssignmentRight => References.Null.ToSingular();

        public override bool IsLValue => false;

        public string ValueText => "null";

        public override string ToString() => this.ValueText;
    }
}
