using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public sealed class Interpretation : Expression
    {
        public Interpretation(Sort sort, object value)
            : base(ExpressionKind.Interpretation, sort, 0)
        {
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));
            Contract.Requires<ArgumentNullException>(value != null, nameof(value));

            this.Value = value;
        }

        public override string DisplayName
        {
            get { return this.Value.ToString(); }
        }

        public object Value { get; private set; }

        protected override Expression GetChild(int index)
        {
            throw new InvalidOperationException();
        }

        protected override void ValidateThis()
        {
            throw new NotImplementedException();
        }
    }
}
