using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public sealed class BooleanModelFactory : ITypeModelFactory
    {
        private static readonly ImmutableArray<Sort> SortRequirements = ImmutableArray.Create(Sort.Bool);

        private BooleanModelFactory()
        {
        }

        public static BooleanModelFactory Instance { get; } = new BooleanModelFactory();

        public ValueModelKind ValueKind => ValueModelKind.Interpretation;

        // TODO: Pre-create upon the initialization (thread safely) instead of caching and publish
        //       (we need to obtain ITypeSymbol for Boolean to do that)
        private static BooleanValueModel True { get; set; }

        private static BooleanValueModel False { get; set; }

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

        public ITypeModel GetExpressionModel(ITypeSymbol type, IEnumerable<Expression> expressions)
        {
            Contract.Requires(this.AreSortsMatching(type, expressions));

            return new BooleanModel(this, type, (BoolHandle)expressions.Single());
        }

        public IValueModel GetValueModel(ITypeSymbol type, IEnumerable<Interpretation> values)
        {
            Contract.Requires(this.AreSortsMatching(type, values));

            var value = values.Single();
            if (value == ExpressionFactory.True)
            {
                if (True == null)
                {
                    True = new BooleanValueModel(this, type, (BoolHandle)value);
                }

                return True;
            }
            else
            {
                Contract.Assert(value == ExpressionFactory.False);

                if (False == null)
                {
                    False = new BooleanValueModel(this, type, (BoolHandle)value);
                }

                return False;
            }
        }

        public IValueModel GetValueModel(ITypeSymbol type, HeapModelLocation location, IHeapModel heap)
        {
            throw new NotSupportedException();
        }

        public IValueModel GetLiteralValueModel(ITypeSymbol type, object literalValue)
        {
            if ((literalValue as bool?) == true)
            {
                return this.GetValueModel(type, ExpressionFactory.True.ToSingular());
            }
            else
            {
                Contract.Assert((literalValue as bool?) == false);

                return this.GetValueModel(type, ExpressionFactory.False.ToSingular());
            }
        }

        public void ModelOperation(IModellingContext context, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            Contract.Requires(context != null);
            Contract.Requires(method != null);
            Contract.Requires(arguments != null);
            Contract.Requires(arguments.Count() == method.Parameters.Length);

            if (method.MethodKind != MethodKind.BuiltinOperator
                || method.Parameters.Length == 0
                || !this.IsTypeSupported(method.ReturnType))
            {
                // This might be the case of static methods etc.
                context.SetUnsupported();
                return;
            }

            BoolHandle boolResult = GetOperationResult(method, arguments);

            if (boolResult.Expression != null)
            {
                Contract.Assert(this.IsTypeSupported(method.ReturnType));
                var resultModel = new BooleanModel(this, method.ReturnType, boolResult);

                context.SetResultValue(resultModel);
            }
            else
            {
                context.SetUnsupported();
            }
        }

        private static BoolHandle GetOperationResult(IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            BoolHandle boolResult;

            var first = ((BooleanModel)arguments.First()).Value;

            if (method.Parameters.Length == 1)
            {
                if (method.Name == "op_LogicalNot")
                {
                    boolResult = !first;
                }
            }
            else
            {
                Contract.Assert(method.Parameters.Length == 2);
                var second = ((BooleanModel)arguments.ElementAt(1)).Value;

                switch (method.Name)
                {
                    case "op_Equality":
                        boolResult = (first == second);
                        break;
                    case "op_Inequality":
                        boolResult = (first != second);
                        break;
                    case "op_BitwiseAnd":
                        boolResult = (first & second);
                        break;
                    case "op_BitwiseOr":
                        boolResult = (first | second);
                        break;
                    default:
                        break;
                }
            }

            return boolResult;
        }
    }
}
