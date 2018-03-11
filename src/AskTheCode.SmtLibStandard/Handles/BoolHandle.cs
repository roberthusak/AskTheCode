using System;
using System.Collections.Generic;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.SmtLibStandard.Handles
{
    public struct BoolHandle : IHandle
    {
        private Expression expression;

        public BoolHandle(Expression boolExpression)
        {
            Contract.Requires<ArgumentNullException>(boolExpression != null, nameof(boolExpression));
            Contract.Requires<ArgumentException>(boolExpression.Sort == Sort.Bool, nameof(boolExpression));

            this.expression = boolExpression;
        }

        public BoolHandle(bool value)
        {
            this.expression = ExpressionFactory.BoolInterpretation(value);
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
                Contract.Requires<ArgumentException>(value.Sort == Sort.Bool, nameof(value));

                this.expression = value;
            }
        }

        public static explicit operator BoolHandle(Expression boolExpression)
        {
            return new BoolHandle(boolExpression);
        }

        public static explicit operator BoolHandle(Handle handle)
        {
            return new BoolHandle(handle.Expression);
        }

        public static implicit operator Handle(BoolHandle boolHandle)
        {
            return new Handle()
            {
                Expression = boolHandle.Expression
            };
        }

        public static implicit operator Expression(BoolHandle handle)
        {
            return handle.Expression;
        }

        public static implicit operator BoolHandle(bool value)
        {
            return new BoolHandle(value);
        }

        public static bool operator true(BoolHandle self)
        {
            // To prevent short-circuit evaluation
            return false;
        }

        public static bool operator false(BoolHandle self)
        {
            // To prevent short-circuit evaluation
            return false;
        }

        public static BoolHandle operator !(BoolHandle handle)
        {
            return (BoolHandle)ExpressionFactory.Not(handle.Expression);
        }

        public static BoolHandle operator &(BoolHandle left, BoolHandle right)
        {
            return (BoolHandle)ExpressionFactory.And(left.Expression, right.Expression);
        }

        public static BoolHandle operator |(BoolHandle left, BoolHandle right)
        {
            return (BoolHandle)ExpressionFactory.Or(left.Expression, right.Expression);
        }

        public static BoolHandle operator ^(BoolHandle left, BoolHandle right)
        {
            return (BoolHandle)ExpressionFactory.Xor(left.Expression, right.Expression);
        }

        public static BoolHandle operator ==(BoolHandle left, BoolHandle right)
        {
            return (BoolHandle)ExpressionFactory.Equal(left.Expression, right.Expression);
        }

        public static BoolHandle operator !=(BoolHandle left, BoolHandle right)
        {
            return (BoolHandle)ExpressionFactory.Distinct(left.Expression, right.Expression);
        }

        public static BoolHandle Implies(BoolHandle left, BoolHandle right)
        {
            return (BoolHandle)ExpressionFactory.Implies(left.Expression, right.Expression);
        }

        public static TValueHandle IfThenElse<TValueHandle>(
            BoolHandle condition,
            TValueHandle valueTrue,
            TValueHandle valueFalse)
            where TValueHandle : struct, IHandle
        {
            Contract.Requires<ArgumentException>(
                valueTrue.Expression.Sort == valueFalse.Expression.Sort,
                nameof(valueFalse));

            var expression = ExpressionFactory.IfThenElse(
                condition.Expression,
                valueTrue.Expression,
                valueFalse.Expression);
            return new TValueHandle()
            {
                Expression = expression
            };
        }

        public BoolHandle Implies(BoolHandle right)
        {
            return Implies(this, right);
        }

        public TValueHandle IfThenElse<TValueHandle>(TValueHandle valueTrue, TValueHandle valueFalse)
            where TValueHandle : struct, IHandle
        {
            return IfThenElse(this, valueTrue, valueFalse);
        }

        public bool TryGetConstantValue(out bool value)
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
