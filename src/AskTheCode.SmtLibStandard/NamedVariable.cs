using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public class NamedVariable : Variable, INamedSymbol
    {
        internal NamedVariable(Sort sort, SymbolName name)
            : base(sort)
        {
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));

            this.Name = name;
        }

        public SymbolName Name { get; private set; }

        protected override string GetName()
        {
            return this.Name.ToString();
        }
    }
}
