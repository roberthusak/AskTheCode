using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeSystem;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class ReferenceModelFactory : ITypeModelFactory
    {
        private static readonly ImmutableArray<Sort> SortRequirements = ImmutableArray.Create(References.Sort);

        public ReferenceModelFactory(ClassDefinition type)
        {
            this.Type = type;
            this.NullModel = new NullReferenceModel(this);
        }

        public ClassDefinition Type { get; }

        public NullReferenceModel NullModel { get; }

        public ValueModelKind ValueKind => ValueModelKind.Reference;

        public bool IsTypeSupported(ITypeSymbol type) => type.Equals(this.Type.Symbol);

        public IReadOnlyList<Sort> GetExpressionSortRequirements(ITypeSymbol type) => SortRequirements;

        public ITypeModel GetExpressionModel(ITypeSymbol type, IEnumerable<Expression> expressions)
        {
            Contract.Requires(type != null, nameof(type));
            Contract.Requires(this.IsTypeSupported(type), nameof(type));
            Contract.Requires(expressions.SingleOrDefault() is Variable);

            return new ReferenceVariableModel(this, (Variable)expressions.Single());
        }

        public IValueModel GetValueModel(ITypeSymbol type, IEnumerable<Interpretation> values)
        {
            throw new NotSupportedException();
        }

        public IValueModel GetValueModel(ITypeSymbol type, HeapModelLocation location, IHeapModel heap)
        {
            Contract.Requires(type != null, nameof(type));
            Contract.Requires(this.IsTypeSupported(type), nameof(type));

            return new ReferenceValueModel(this, location, heap);
        }

        public IValueModel GetLiteralValueModel(ITypeSymbol type, object literalValue)
        {
            Contract.Requires<ArgumentException>(literalValue == null, nameof(literalValue));

            return this.NullModel;
        }

        public void ModelOperation(IModellingContext context, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            if (method.MethodKind != MethodKind.BuiltinOperator
                || method.Parameters.Length != 2)
            {
                context.SetUnsupported();
                return;
            }

            var boolResult = this.GetOperationResult(context, method, arguments);

            if (boolResult.Expression != null)
            {
                var resultModel = BooleanModelFactory.Instance.GetExpressionModel(
                    method.ReturnType,
                    boolResult.Expression.ToSingular());
                context.SetResultValue(resultModel);
            }
            else
            {
                context.SetUnsupported();
            }
        }

        private BoolHandle GetOperationResult(
            IModellingContext context,
            IMethodSymbol method,
            IEnumerable<ITypeModel> arguments)
        {
            if (method.Name == "op_Equality")
            {
                return (BoolHandle)ExpressionFactory.Equal(
                    arguments.First().AssignmentRight[0],
                    arguments.ElementAt(1).AssignmentRight[0]);
            }
            else if (method.Name == "op_Inequality")
            {
                return (BoolHandle)ExpressionFactory.Distinct(
                    arguments.First().AssignmentRight[0],
                    arguments.ElementAt(1).AssignmentRight[0]);
            }
            else
            {
                return default(BoolHandle);
            }
        }
    }
}
