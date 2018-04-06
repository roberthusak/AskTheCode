using System;
using System.Collections.Generic;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.SmtLibStandard.Handles
{
    public struct ArrayHandle<TKeyHandle, TValueHandle> : IHandle
        where TKeyHandle : struct, IHandle
        where TValueHandle : struct, IHandle
    {
        private Expression expression;

        public ArrayHandle(Expression arrayExpression)
        {
            Contract.Requires<ArgumentNullException>(arrayExpression != null, nameof(arrayExpression));
            Contract.Requires<ArgumentException>(arrayExpression.Sort.IsArray, nameof(arrayExpression));

            this.expression = arrayExpression;
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
                Contract.Requires<ArgumentException>(value.Sort.IsArray, nameof(value));

                this.expression = value;
            }
        }

        public static explicit operator ArrayHandle<TKeyHandle, TValueHandle>(Expression arrayExpression)
        {
            return new ArrayHandle<TKeyHandle, TValueHandle>(arrayExpression);
        }

        public static explicit operator ArrayHandle<TKeyHandle, TValueHandle>(Handle handle)
        {
            return new ArrayHandle<TKeyHandle, TValueHandle>(handle.Expression);
        }

        public static implicit operator Handle(ArrayHandle<TKeyHandle, TValueHandle> intHandle)
        {
            return new Handle()
            {
                Expression = intHandle.Expression
            };
        }

        public static implicit operator Expression(ArrayHandle<TKeyHandle, TValueHandle> handle)
        {
            return handle.Expression;
        }

        public static BoolHandle operator ==(ArrayHandle<TKeyHandle, TValueHandle> left, ArrayHandle<TKeyHandle, TValueHandle> right)
        {
            return (BoolHandle)ExpressionFactory.Equal(left.Expression, right.Expression);
        }

        public static BoolHandle operator !=(ArrayHandle<TKeyHandle, TValueHandle> left, ArrayHandle<TKeyHandle, TValueHandle> right)
        {
            return (BoolHandle)ExpressionFactory.Distinct(left.Expression, right.Expression);
        }

        public static TValueHandle Select<TKeyHandle, TValueHandle>(
            ArrayHandle<TKeyHandle, TValueHandle> array,
            TKeyHandle key)
            where TKeyHandle : struct, IHandle
            where TValueHandle : struct, IHandle
        {
            var expression = ExpressionFactory.Select(array.Expression, key.Expression);

            return new TValueHandle()
            {
                Expression = expression
            };
        }

        public static ArrayHandle<TKeyHandle, TValueHandle> Store<TKeyHandle, TValueHandle>(
            ArrayHandle<TKeyHandle, TValueHandle> array,
            TKeyHandle key,
            TValueHandle value)
            where TKeyHandle : struct, IHandle
            where TValueHandle : struct, IHandle
        {
            var expression = ExpressionFactory.Store(array.Expression, key.Expression, value.Expression);

            return (ArrayHandle<TKeyHandle, TValueHandle>)expression;
        }

        public TValueHandle Select(TKeyHandle key)
        {
            return Select(this, key);
        }

        public ArrayHandle<TKeyHandle, TValueHandle> Store(TKeyHandle key, TValueHandle value)
        {
            return Store(this, key, value);
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
