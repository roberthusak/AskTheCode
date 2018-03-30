using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeSystem
{
    internal class TypeContext
    {
        private readonly TypeModelManager modelManager;

        private readonly ConcurrentDictionary<ITypeSymbol, ClassDefinition> typeSymbolToDefinitionMap =
            new ConcurrentDictionary<ITypeSymbol, ClassDefinition>();

        private readonly ConcurrentDictionary<IFieldSymbol, ImmutableArray<FieldDefinition>> fieldSymbolToDefinitionsMap =
            new ConcurrentDictionary<IFieldSymbol, ImmutableArray<FieldDefinition>>();

        public TypeContext(TypeModelManager modelManager)
        {
            this.modelManager = modelManager;
        }

        public ClassDefinition GetClassDefinition(ITypeSymbol symbol)
        {
            Contract.Requires(symbol != null);

            return this.typeSymbolToDefinitionMap.GetOrAdd(symbol, this.CreateClassDefinition);
        }

        public ImmutableArray<FieldDefinition> GetFieldDefinitions(IFieldSymbol symbol)
        {
            Contract.Requires(symbol != null);

            return this.fieldSymbolToDefinitionsMap.GetOrAdd(symbol, this.CreateFieldDefinitions);
        }

        private ClassDefinition CreateClassDefinition(ITypeSymbol symbol)
        {
            return new ClassDefinition(this, symbol);
        }

        private ImmutableArray<FieldDefinition> CreateFieldDefinitions(IFieldSymbol symbol)
        {
            var result = new List<FieldDefinition>();

            var modelFactory = this.modelManager.TryGetFactory(symbol.Type);
            if (modelFactory != null)
            {
                var sorts = modelFactory.GetExpressionSortRequirements(symbol.Type);
                for (int i = 0; i < sorts.Count; i++)
                {
                    int? orderNumber = (sorts.Count == 0) ? (int?)null : i;
                    result.Add(new FieldDefinition(symbol, sorts[i], orderNumber));
                }
            }
            else if (symbol.Type.IsReferenceType)
            {
                var referencedClass = this.GetClassDefinition(symbol.Type);
                result.Add(new FieldDefinition(symbol, referencedClass));
            }
            else
            {
                // TODO: Store a warning somewhere
            }

            return result.ToImmutableArray();
        }
    }
}
