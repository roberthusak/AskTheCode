using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class IntegerValueModel : IntegerModel, IValueModel
    {
        internal IntegerValueModel(IntegerModelFactory factory, ITypeSymbol type, IntHandle value)
            : base(factory, type, value)
        {
        }
    }
}
