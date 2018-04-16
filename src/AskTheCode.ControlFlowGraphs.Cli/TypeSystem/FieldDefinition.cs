using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeSystem
{
    public class FieldDefinition : IFieldDefinition
    {
        private int? orderNumber;

        internal FieldDefinition(IFieldSymbol symbol, Sort sort, int? orderNumber = null)
        {
            Contract.Requires(symbol != null);
            Contract.Requires(sort != null);

            this.Symbol = symbol;
            this.Sort = sort;
            this.orderNumber = orderNumber;
        }

        internal FieldDefinition(IFieldSymbol symbol, IClassDefinition referencedClass)
        {
            Contract.Requires(symbol != null);
            Contract.Requires(referencedClass != null);

            this.Symbol = symbol;
            this.Sort = References.Sort;
            this.ReferencedClass = referencedClass;
        }

        public Sort Sort { get; }

        public IClassDefinition ReferencedClass { get; }

        internal IFieldSymbol Symbol { get; }
    }
}