using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public enum ValueModelKind
    {
        Interpretation,
        Reference
    }

    public interface ITypeModelFactory
    {
        ValueModelKind ValueKind { get; }

        bool IsTypeSupported(ITypeSymbol type);

        IReadOnlyList<Sort> GetExpressionSortRequirements(ITypeSymbol type);

        ITypeModel GetExpressionModel(ITypeSymbol type, IEnumerable<Expression> expressions);

        IValueModel GetValueModel(ITypeSymbol type, IEnumerable<Interpretation> values);

        IValueModel GetValueModel(ITypeSymbol type, HeapModelLocation location, IHeapModel heap);

        void ModelOperation(IModellingContext context, IMethodSymbol method, IEnumerable<ITypeModel> arguments);

        IValueModel GetLiteralValueModel(ITypeSymbol type, object literalValue);

        // TODO: Incorporate the usage of global variables
    }

    public static class TypeModelFactoryExtensions
    {
        public static bool AreSortsMatching(this ITypeModelFactory self, ITypeSymbol type, IEnumerable<Expression> expressions)
        {
            Contract.Requires(self != null, nameof(self));
            Contract.Requires(type != null, nameof(type));
            Contract.Requires(expressions != null, nameof(expressions));

            var requirements = self.GetExpressionSortRequirements(type);
            Contract.Assert(requirements != null);

            if (expressions.Count() != requirements.Count)
            {
                return false;
            }

            int i = 0;
            foreach (var expression in expressions)
            {
                Contract.Assert(expression != null);

                if (expression.Sort != requirements[i])
                {
                    return false;
                }

                i++;
            }

            return true;
        }
    }

    [Serializable]
    public class NotSupportedTypeException : InvalidOperationException
    {
        public NotSupportedTypeException()
        {
        }

        public NotSupportedTypeException(string message)
            : base(message)
        {
        }

        public NotSupportedTypeException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected NotSupportedTypeException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {
        }
    }
}
