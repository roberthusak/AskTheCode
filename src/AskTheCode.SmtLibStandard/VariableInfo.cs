using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public struct VariableInfo
    {
        public VariableInfo(Variable variable, SymbolName name)
        {
            Contract.Requires<ArgumentNullException>(variable != null, nameof(variable));
            Contract.Requires<ArgumentException>(name.IsValid, nameof(name));

            this.Variable = variable;
            this.Name = name;
        }

        public Variable Variable { get; private set; }

        public SymbolName Name { get; private set; }

        public bool IsValid
        {
            get { return (this.Variable != null && this.Name.IsValid); }
        }
    }
}
