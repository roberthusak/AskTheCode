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
    public class BooleanModelFactory : ITypeModelFactory
    {
        private static readonly ImmutableArray<Sort> SortRequirements = ImmutableArray.Create(Sort.Bool);

        public bool IsTypeSupported(ITypeSymbol type)
        {
            Contract.Requires(type != null, nameof(type));

            return type.SpecialType == SpecialType.System_Boolean;
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

            return new BooleanModel(this, type, (BoolHandle)expressions.Single());
        }

        public void ModelOperation(IOperationModellingContext context, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            // TODO
        }
    }
}
