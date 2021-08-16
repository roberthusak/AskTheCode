using System;
using CodeContractsRevival.Runtime;

namespace AskTheCode.Core
{
    public sealed class InspectionConditions
    {
        internal InspectionConditions(string expression)
        {
            Contract.Requires<ArgumentNullException>(expression != null, nameof(expression));

            this.Expression = expression;
        }

        public string Expression { get; private set; }
    }
}