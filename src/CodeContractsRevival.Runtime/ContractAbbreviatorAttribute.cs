using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace CodeContractsRevival.Runtime
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    [Conditional("DEBUG")]
    public sealed class ContractAbbreviatorAttribute : Attribute
    {
        public ContractAbbreviatorAttribute() { }
    }
}
