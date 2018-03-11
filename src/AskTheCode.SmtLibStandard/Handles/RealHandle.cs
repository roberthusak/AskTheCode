using System;
using System.Collections.Generic;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.SmtLibStandard.Handles
{
    // TODO: Implement after solving its interoperability with Int (conversions, divisions etc.)
    public struct RealHandle : IHandle
    {
        private Expression expression;

        public RealHandle(Expression realExpression)
        {
            Contract.Requires<ArgumentNullException>(realExpression != null, nameof(realExpression));
            Contract.Requires<ArgumentException>(realExpression.Sort == Sort.Real, nameof(realExpression));

            this.expression = realExpression;
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
                Contract.Requires<ArgumentException>(value.Sort == Sort.Real, nameof(value));

                this.expression = value;
            }
        }

        public static explicit operator RealHandle(Expression realExpression)
        {
            return new RealHandle(realExpression);
        }

        public static explicit operator RealHandle(Handle handle)
        {
            return new RealHandle(handle.Expression);
        }

        public static implicit operator Handle(RealHandle realHandle)
        {
            return new Handle()
            {
                Expression = realHandle.Expression
            };
        }

        public static implicit operator Expression(RealHandle handle)
        {
            return handle.Expression;
        }

        public override string ToString()
        {
            if (this.Expression == null)
            {
                return "<null>";
            }
            else
            {
                return this.Expression.ToString();
            }
        }
    }
}
