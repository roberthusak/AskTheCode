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
            Contract.Requires(method.Locations.Length == 1, nameof(method));

            this.Method = method;

            if (isExplorationDisabled)
            {
                this.CanBeExplored = false;
            }
            else
            {
                this.CanBeExplored = this.Method.Locations.Single().IsInSource;
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
