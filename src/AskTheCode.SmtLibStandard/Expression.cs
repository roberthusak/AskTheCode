using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.SmtLibStandard
{
    /// <summary>
    /// The basic abstract class of all expressions in SMT-LIB.
    /// </summary>
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

        /// <summary>
        /// Gets the kind of the expression.
        /// </summary>
        public ExpressionKind Kind { get; private set; }

        /// <summary>
        /// Gets the SMT-LIB symbolic type of the expression.
        /// </summary>
        public Sort Sort { get; private set; }

        /// <summary>
        /// Gets the number of child expressions.
        /// </summary>
        public int ChildrenCount { get; private set; }

        /// <summary>
        /// Gets the identifier to display in the Lisp-like syntax of SMT-LIB.
        /// </summary>
        public abstract string DisplayName { get; }

        /// <summary>
        /// Get the children expressions.
        /// </summary>
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

        /// <summary>
        /// Accepts a visitor without a return value.
        /// </summary>
        public abstract void Accept(ExpressionVisitor visitor);

        /// <summary>
        /// Acceptrs a visitor with a return value.
        /// </summary>
        public abstract TResult Accept<TResult>(ExpressionVisitor<TResult> visitor);

        public void Validate()
        {
            this.ValidateThis();
            foreach (var child in this.Children)
            {
                child.Validate();
            }
        }

        /// <summary>
        /// Returns a string in Lisp-like syntax that represents the current object.
        /// </summary>
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

        /// <summary>
        /// Obtains the child on the specified index.
        /// </summary>
        public abstract Expression GetChild(int index);

        /// <summary>
        /// Recursively check the consistency of the expression.
        /// </summary>
        /// <remarks>
        /// Currently not being used.
        /// </remarks>
        protected abstract void ValidateThis();
    }
}
