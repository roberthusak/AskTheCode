using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    /// <summary>
    /// Represents a base for a variable in SMT-LIB (usually called "constant").
    /// </summary>
    /// <remarks>
    /// It is named this way, because with the name "Constant" one could expect its semantics to be as the one of
    /// <see cref="Interpretation"/>. It is abstract in order to allow creating own variable types and reusing the
    /// same expressions multiple times just by providing an <see cref="INameProvider"/>. To create a variable that
    /// keeps a constant name, use <see cref="NamedVariable"/>.
    /// </remarks>
    public abstract class Variable : Expression
    {
        protected Variable(Sort sort)
            : base(ExpressionKind.Variable, sort, 0)
        {
        }

        public override void Accept(ExpressionVisitor visitor)
        {
            visitor.VisitVariable(this);
        }

        public override TResult Accept<TResult>(ExpressionVisitor<TResult> visitor)
        {
            return visitor.VisitVariable(this);
        }

        public sealed override Expression GetChild(int index)
        {
            throw new InvalidOperationException();
        }

        protected sealed override void ValidateThis()
        {
            throw new NotImplementedException();
        }
    }
}
