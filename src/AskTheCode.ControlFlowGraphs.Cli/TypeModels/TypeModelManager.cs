using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    // TODO: Add dynamic factories registering
    // TODO: Add working with global variables
    internal class TypeModelManager
    {
        private BooleanModelFactory booleanFactory = new BooleanModelFactory();
        private IntegerModelFactory integerFactory = new IntegerModelFactory();

        public ITypeModelFactory TryGetFactory(ITypeSymbol type)
        {
            Contract.Requires(type != null);

            if (this.booleanFactory.IsTypeSupported(type))
            {
                return this.booleanFactory;
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
