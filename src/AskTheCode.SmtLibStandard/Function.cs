using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public sealed class Function : Expression
    {
        private readonly Expression[] children;

        internal Function(ExpressionKind kind, Sort sort, params Expression[] children)
            : base(kind, sort, children.Length)
        {
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));
            Contract.Requires<ArgumentNullException>(children != null, nameof(children));

            this.children = children;
        }

        public static string GetName(ExpressionKind kind)
        {
            switch (kind)
            {
                case ExpressionKind.Not:
                    return "not";
                case ExpressionKind.And:
                    return "and";
                case ExpressionKind.Or:
                    return "or";
                case ExpressionKind.Xor:
                    return "xor";
                case ExpressionKind.Implies:
                    return "=>";

                case ExpressionKind.Negate:
                    return "-";
                case ExpressionKind.Multiply:
                    return "*";
                case ExpressionKind.DivideReal:
                    return "/";
                case ExpressionKind.DivideInteger:
                    return "div";
                case ExpressionKind.Modulus:
                    return "mod";
                case ExpressionKind.Remainder:
                    return "rem";
                case ExpressionKind.Add:
                    return "+";
                case ExpressionKind.Subtract:
                    return "-";
                case ExpressionKind.LessThan:
                    return "<";
                case ExpressionKind.GreaterThan:
                    return ">";
                case ExpressionKind.LessThanOrEqual:
                    return "<=";
                case ExpressionKind.GreaterThanOrEqual:
                    return ">=";

                case ExpressionKind.Equal:
                    return "=";
                case ExpressionKind.Distinct:
                    return "distinct";
                case ExpressionKind.IfThenElse:
                    return "ite";

                case ExpressionKind.Interpretation:
                case ExpressionKind.Variable:
                default:
                    // TODO: Add some descprition of the error (and put it to the resources)
                    throw new ArgumentException(nameof(kind));
            }
        }

        protected override Expression GetChild(int index)
        {
            return this.children[index];
        }

        protected override string GetName()
        {
            return GetName(this.Kind);
        }

        protected override void ValidateThis()
        {
            throw new NotImplementedException();
        }
    }
}
