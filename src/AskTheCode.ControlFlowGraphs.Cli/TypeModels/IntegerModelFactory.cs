using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class IntegerModelFactory : ITypeModelFactory
    {
        private static readonly ImmutableArray<Sort> SortRequirements = ImmutableArray.Create(Sort.Bool);

        public bool IsTypeSupported(ITypeSymbol type)
        {
            Contract.Requires(type != null, nameof(type));

            switch (type.SpecialType)
            {
                case SpecialType.System_Char:
                case SpecialType.System_SByte:
                case SpecialType.System_Byte:
                case SpecialType.System_Int16:
                case SpecialType.System_UInt16:
                case SpecialType.System_Int32:
                case SpecialType.System_UInt32:
                case SpecialType.System_Int64:
                case SpecialType.System_UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public IReadOnlyList<Sort> GetExpressionSortRequirements(ITypeSymbol type)
        {
            Contract.Requires(type != null, nameof(type));
            Contract.Requires(this.IsTypeSupported(type), nameof(type));

            return SortRequirements;
        }

        public ITypeModel GetVariableModel(ITypeSymbol type, IEnumerable<Expression> expressions)
        {
            Contract.Requires(this.AreSortsMatching(type, expressions));

            return new IntegerModel(this, type, (IntHandle)expressions.Single());
        }

        // TODO: Somehow handle the different sizes and signed/unsigned variants using modulo etc.
        public void ModelOperation(IOperationModellingContext context, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            Contract.Requires(context != null);
            Contract.Requires(method != null);
            Contract.Requires(arguments != null);

            // TODO: distinguish the particular operator

            // Assume + for now
            Contract.Assert(arguments.Count() == 2);
            var left = (IntegerModel)arguments.First();
            var right = (IntegerModel)arguments.ElementAt(1);
            context.SetValue(left.Value + right.Value);
        }
    }
}
