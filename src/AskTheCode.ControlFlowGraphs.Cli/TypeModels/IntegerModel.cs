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
    public class IntegerModel : ITypeModel
    {
        internal IntegerModel(IntegerModelFactory factory, ITypeSymbol type, IntHandle value)
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

                return new SingleReadOnlyList<Variable>((Variable)this.Value.Expression);
            }
        }

        public IReadOnlyList<Expression> AssignmentRight
        {
            get
            {
                return new SingleReadOnlyList<Expression>(this.Value.Expression);
            }
        }

        public ITypeModelFactory Factory { get; private set; }

        public bool IsLValue
        {
            get { return this.Value.Expression is Variable; }
        }

        public ITypeSymbol Type { get; private set; }

        public IntHandle Value { get; private set; }
    }
}
