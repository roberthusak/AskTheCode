using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    /// <summary>
    /// Models a reference.
    /// </summary>
    public abstract class ReferenceModel : ITypeModel
    {
        internal ReferenceModel(ReferenceModelFactory factory)
        {
            this.Factory = factory;
        }

        public abstract IReadOnlyList<Variable> AssignmentLeft { get; }

        public abstract IReadOnlyList<Expression> AssignmentRight { get; }

        public ReferenceModelFactory Factory { get; }

        ITypeModelFactory ITypeModel.Factory => this.Factory;

        public abstract bool IsLValue { get; }

        public ITypeSymbol Type => this.Factory.Type.Symbol;
    }
}
