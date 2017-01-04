using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    /// <summary>
    /// Denotes the kind of the expression.
    /// </summary>
    /// <remarks>
    /// It is possible to recognize certain types of expressions by their runtime types. However, to exactly determine
    /// the actual semantics, this enumeration is needed.
    /// </remarks>
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

        // TODO: Consider renaming Negate to Inverse (to differentiate from Not)
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
        Distinct,
        IfThenElse
    }
}
