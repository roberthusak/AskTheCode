using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class BooleanValueModel : BooleanModel, IValueModel
    {
        internal BooleanValueModel(BooleanModelFactory factory, ITypeSymbol type, BoolHandle value)
            : base(factory, type, value)
        {
            Contract.Requires(value.Expression.Kind == ExpressionKind.Interpretation);
        }

        public string ValueText
        {
            get
            {
                var interpretation = (Interpretation)this.Value.Expression;
                return interpretation.DisplayName;
            }
        }
    }
}
