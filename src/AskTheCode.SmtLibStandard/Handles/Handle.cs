using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard.Handles
{
    public struct Handle : IHandle
    {
        private Expression expression;

        public Handle(Expression expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null, nameof(expression));

            this.expression = expression;
        }

        public Expression Expression
        {
            get
            {
                return this.expression;
            }

            set
            {
                Contract.Requires<ArgumentNullException>(value != null, nameof(value));

                this.expression = value;
            }
        }

        public static explicit operator Handle(Expression expression)
        {
            return new Handle(expression);
        }

        public static implicit operator Expression(Handle handle)
        {
            return handle.Expression;
        }
    }
}
