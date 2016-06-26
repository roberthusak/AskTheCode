using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
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

        protected internal sealed override Expression GetChild(int index)
        {
            throw new InvalidOperationException();
        }

        protected sealed override void ValidateThis()
        {
            throw new NotImplementedException();
        }
    }
}
