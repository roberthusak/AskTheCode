using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    public class MethodLocation : ILocation
    {
        public MethodLocation(IMethodSymbol method, bool canBeExplored)
        {
            Contract.Requires<ArgumentNullException>(method != null, nameof(method));

            this.Method = method;
            this.CanBeExplored = canBeExplored;
        }

        public IMethodSymbol Method { get; private set; }

        public bool CanBeExplored { get; private set; }

        public override string ToString()
        {
            return $"{this.Method.ContainingType.Name}.{this.Method.Name}";
        }
    }
}
