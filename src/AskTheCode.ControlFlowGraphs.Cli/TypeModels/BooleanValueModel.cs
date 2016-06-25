using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class BooleanValueModel : BooleanModel, IValueModel
    {
        internal BooleanValueModel(BooleanModelFactory factory, ITypeSymbol type, BoolHandle value)
            : base(factory, type, value)
        {
        }
    }
}
