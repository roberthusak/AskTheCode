using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard.Handles
{
    public struct IntHandle : IHandle
    {
        private Expression expression;

        public IntHandle(Expression intExpression)
        {
            Contract.Requires<ArgumentNullException>(intExpression != null, nameof(intExpression));
            Contract.Requires<ArgumentException>(intExpression.Sort == Sort.Int, nameof(intExpression));

            this.expression = intExpression;
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
                Contract.Requires<ArgumentException>(value.Sort == Sort.Int, nameof(value));

                this.expression = value;
            }
        }

        public static explicit operator IntHandle(Expression intExpression)
        {
            return new IntHandle(intExpression);
        }

        public static explicit operator IntHandle(Handle handle)
        {
            return new IntHandle(handle.Expression);
        }

        public static implicit operator Handle(IntHandle intHandle)
        {
            return new Handle()
            {
                Expression = intHandle.Expression
            };
        }

        public static implicit operator Expression(IntHandle handle)
        {
            return handle.Expression;
        }

        public static IntHandle operator -(IntHandle handle)
        {
            return (IntHandle)ExpressionFactory.Negate(handle.Expression);
        }

        public static IntHandle operator *(IntHandle left, IntHandle right)
        {
            return (IntHandle)ExpressionFactory.Multiply(left.Expression, right.Expression);
        }

        public static IntHandle operator /(IntHandle left, IntHandle right)
        {
            return (IntHandle)ExpressionFactory.DivideInteger(left.Expression, right.Expression);
        }

        public static IntHandle operator %(IntHandle left, IntHandle right)
        {
            return (IntHandle)ExpressionFactory.Modulus(left.Expression, right.Expression);
        }

        public static IntHandle operator +(IntHandle left, IntHandle right)
        {
            return (IntHandle)ExpressionFactory.Add(left.Expression, right.Expression);
        }

        public static IntHandle operator -(IntHandle left, IntHandle right)
        {
            return (IntHandle)ExpressionFactory.Subtract(left.Expression, right.Expression);
        }

        public static BoolHandle operator <(IntHandle left, IntHandle right)
        {
            return (BoolHandle)ExpressionFactory.LessThan(left.Expression, right.Expression);
        }

        public static BoolHandle operator >(IntHandle left, IntHandle right)
        {
            return (BoolHandle)ExpressionFactory.GreaterThan(left.Expression, right.Expression);
        }

        public static BoolHandle operator <=(IntHandle left, IntHandle right)
        {
            return (BoolHandle)ExpressionFactory.LessThanOrEqual(left.Expression, right.Expression);
        }

        public static BoolHandle operator >=(IntHandle left, IntHandle right)
        {
            return (BoolHandle)ExpressionFactory.GreaterThanOrEqual(left.Expression, right.Expression);
        }

        public static BoolHandle operator ==(IntHandle left, IntHandle right)
        {
            return (BoolHandle)ExpressionFactory.Equal(left.Expression, right.Expression);
        }

        public static BoolHandle operator !=(IntHandle left, IntHandle right)
        {
            return (BoolHandle)ExpressionFactory.Distinct(left.Expression, right.Expression);
        }

        public static RealHandle DivideReal(IntHandle left, IntHandle right)
        {
            return (RealHandle)ExpressionFactory.DivideReal(left.Expression, right.Expression);
        }

        public static IntHandle Remainder(IntHandle left, IntHandle right)
        {
            return (IntHandle)ExpressionFactory.Remainder(left.Expression, right.Expression);
        }

        public RealHandle DivideReal(IntHandle right)
        {
            return DivideReal(this, right);
        }

        public IntHandle Remainder(IntHandle right)
        {
            return Remainder(this, right);
        }

        public bool TryGetConstantValue(out long value)
        {
            throw new NotImplementedException();
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
