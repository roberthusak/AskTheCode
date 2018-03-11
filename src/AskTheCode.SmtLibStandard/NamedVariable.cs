using System;
using System.Collections.Generic;
using System.Text;
using CodeContractsRevival.Runtime;

namespace AskTheCode.SmtLibStandard
{
    /// <summary>
    /// A variable with a constant name.
    /// </summary>
    public class NamedVariable : Variable, INamedSymbol
    {
        internal NamedVariable(Sort sort, SymbolName name)
            : base(sort)
        {
            Contract.Requires<ArgumentNullException>(sort != null, nameof(sort));

            this.SymbolName = name;
        }

        public override string DisplayName
        {
            get { return this.SymbolName.ToString(); }
        }

        public SymbolName SymbolName { get; private set; }
    }
}
