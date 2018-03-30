using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.TypeSystem;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeSystem
{
    public class ClassDefinition : IClassDefinition
    {
        private readonly TypeContext context;

        internal ClassDefinition(TypeContext context, ITypeSymbol symbol)
        {
            this.context = context;
            this.Symbol = symbol;

            this.Fields = new AsyncLazy<IEnumerable<IFieldDefinition>>(() => Task.FromResult(this.GetFields()));
        }

        public AsyncLazy<IEnumerable<IFieldDefinition>> Fields { get; }

        internal ITypeSymbol Symbol { get; }

        public override string ToString() => this.Symbol.Name;

        private IEnumerable<IFieldDefinition> GetFields()
        {
            return this.Symbol
                .GetBaseTypesAndThis()
                .SelectMany(t => t.GetMembers())
                .OfType<IFieldSymbol>()
                .SelectMany(s => this.context.GetFieldDefinitions(s))
                .ToImmutableArray();
        }
    }
}