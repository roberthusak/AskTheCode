using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public interface IAssertionStackLevel
    {
        IEnumerable<VariableInfo> DeclaredVariables { get; }

        IEnumerable<Expression> Assertions { get; }
    }
}
