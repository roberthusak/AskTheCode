using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public enum ExpressionKind
    {
        // TODO: Add bitvector, array and sequence theories functions (and finish the Bool, Int and Real, if anything
        //       is mising)
        Interpretation,
        Variable,

        Not,
        And,
        Or,
        Xor,
        Implies,
        IfThenElse,

        Negate,
        Multiply,
        DivideReal,
        DivideInteger,
        Modulus,
        Remainder,
        Add,
        Subtract,
        LessThan,
        GreaterThan,
        LessThanOrEqual,
        GreaterThanOrEqual,
        Equal,
        Distinct
    }
}
