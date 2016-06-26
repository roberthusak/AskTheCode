using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public abstract class Expression
    {
        internal Expression(ExpressionKind kind, Sort sort, int childrenCount)
        {
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));
            Contract.Requires<ArgumentException>(childrenCount >= 0, nameof(childrenCount));

            this.Kind = kind;
            this.Sort = sort;
            this.ChildrenCount = childrenCount;
        }

        public ExpressionKind Kind { get; private set; }

        public Sort Sort { get; private set; }

        public int ChildrenCount { get; private set; }

        public abstract string DisplayName { get; }

        // TODO: Consider publishing them as an array (immutable, if possible) or at least provide indexing
        public IEnumerable<Expression> Children
        {
            get
            {
                for (int i = 0; i < this.ChildrenCount; i++)
                {
                    yield return this.GetChild(i);
                }
            }
        }

        public abstract void Accept(ExpressionVisitor visitor);

        public abstract TResult Accept<TResult>(ExpressionVisitor<TResult> visitor);

        public void Validate()
        {
            this.ValidateThis();
            foreach (var child in this.Children)
            {
                child.Validate();
            }
        }

        public override string ToString()
        {
            if (this.ChildrenCount == 0)
            {
                return this.DisplayName;
            }
            else
            {
                string childrenNames = string.Join(
                    " ",
                    this.Children.Select(child => child.ToString()));
                return $"({this.DisplayName} {childrenNames})";
            }
        }

        protected internal abstract Expression GetChild(int index);

        protected abstract void ValidateThis();
    }
}
