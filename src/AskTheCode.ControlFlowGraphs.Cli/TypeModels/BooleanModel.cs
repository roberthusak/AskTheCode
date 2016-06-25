using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    // TODO: Move the contracts to the interface if possible
    public class BooleanModel : ITypeModel
    {
        internal BooleanModel(BooleanModelFactory factory, ITypeSymbol type, BoolHandle value)
        {
            Contract.Requires<ArgumentNullException>(factory != null, nameof(factory));
            Contract.Requires<ArgumentException>(value.Expression != null, nameof(value));

            this.Factory = factory;
            this.Type = type;
            this.Value = value;
        }

        public IReadOnlyList<Variable> AssignmentLeft
        {
            get
            {
                Contract.Requires<InvalidAssignmentModelException>(this.IsLValue);

                return ((Variable)this.Value.Expression).ToSingular();
            }
        }

        public IReadOnlyList<Expression> AssignmentRight
        {
            get
            {
                return this.Value.Expression.ToSingular();
            }
        }

        public ITypeModelFactory Factory { get; private set; }

        public bool IsLValue
        {
            get { return this.Value.Expression is Variable; }
        }

        public ITypeSymbol Type { get; private set; }

        public BoolHandle Value { get; private set; }
    }
}
