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
        public MethodLocation(IMethodSymbol method, bool isExplorationDisabled = false)
        {
            Contract.Requires<ArgumentNullException>(method != null, nameof(method));

            this.Method = method;

            if (isExplorationDisabled)
            {
                this.CanBeExplored = false;
            }
            else
            {
                // Methods such as built-in operators do not have locations
                this.CanBeExplored = this.Method.Locations.SingleOrDefault()?.IsInSource == true;
            }
        }

        public IMethodSymbol Method { get; private set; }

        public bool CanBeExplored { get; private set; }

        public override string ToString()
        {
            return $"{this.Method.ContainingType.Name}.{this.Method.Name}";
        }

        // TODO: Consider implementing also == operator
        public bool Equals(MethodLocation other)
        {
            return this.Method.Equals(other.Method);
        }
    }
}
