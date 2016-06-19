using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    // TODO: Handle also global variables
    internal enum VariableOrigin
    {
        Temporary,
        Parameter,
        Local
    }

    internal class BuildVariable : Variable, IIdReferenced<BuildVariableId>
    {
        public BuildVariable(
            BuildVariableId id,
            Sort sort,
            ISymbol symbol,
            VariableOrigin origin)
            : base(sort)
        {
            this.Id = id;
            this.Symbol = symbol;
            this.Origin = origin;
        }

        public override string DisplayName
        {
            get
            {
                if (this.Symbol == null)
                {
                    return $"tmp<{this.Sort.Name}>!{this.Id.Value}";
                }
                else
                {
                    return $"{this.Symbol.Name}<{this.Sort.Name}>!{this.Id.Value}";
                }
            }
        }

        public BuildVariableId Id { get; private set; }

        public ISymbol Symbol { get; private set; }

        public VariableOrigin Origin { get; private set; }
    }
}
