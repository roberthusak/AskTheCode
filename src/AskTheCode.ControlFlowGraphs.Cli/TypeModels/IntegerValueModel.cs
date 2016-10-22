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
    public class IntegerValueModel : IntegerModel, IValueModel
    {
        internal IntegerValueModel(IntegerModelFactory factory, ITypeSymbol type, IntHandle value)
            : base(factory, type, value)
        {
            Contract.Requires(value.Expression.Kind == SmtLibStandard.ExpressionKind.Interpretation);
        }

        public string ValueText
        {
            get
            {
                var interpretation = (Interpretation)this.Value.Expression;
                return interpretation.DisplayName;

                // TODO: Reflect the various types and their ranges (consider starting with the code below)

                ////long longValue;
                ////bool result = this.Value.TryGetConstantValue(out longValue);
                ////Contract.Assert(result);

                ////return longValue.ToString();
            }
        }
    }
}
