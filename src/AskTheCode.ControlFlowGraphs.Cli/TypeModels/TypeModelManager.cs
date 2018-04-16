using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeSystem;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    // TODO: Add dynamic factories registering
    // TODO: Add working with global variables
    public class TypeModelManager
    {
        private readonly TypeContext typeContext;

        private readonly ConcurrentDictionary<ITypeSymbol, ReferenceModelFactory> classToFactoryMap =
            new ConcurrentDictionary<ITypeSymbol, ReferenceModelFactory>();

        private readonly IntegerModelFactory integerFactory = new IntegerModelFactory();

        public TypeModelManager()
        {
            this.typeContext = new TypeContext(this);
        }

        public ITypeModelFactory TryGetFactory(ITypeSymbol type)
        {
            Contract.Requires(type != null);

            if (type.IsReferenceType)
            {
                return this.classToFactoryMap.GetOrAdd(
                    type,
                    t => new ReferenceModelFactory(this.typeContext.GetClassDefinition(t)));
            }
            else if (BooleanModelFactory.Instance.IsTypeSupported(type))
            {
                return BooleanModelFactory.Instance;
            }
            else if (this.integerFactory.IsTypeSupported(type))
            {
                return this.integerFactory;
            }
            else
            {
                return null;
            }
        }
    }
}
