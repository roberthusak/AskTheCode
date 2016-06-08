using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public interface INamedSymbol
    {
        SymbolName Name { get; }
    }
}
