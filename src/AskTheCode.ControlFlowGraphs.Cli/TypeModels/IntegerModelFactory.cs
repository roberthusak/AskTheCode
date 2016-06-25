using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli.TypeModels
{
    public class IntegerModelFactory : ITypeModelFactory
    {
        private static readonly ImmutableArray<Sort> SortRequirements = ImmutableArray.Create(Sort.Int);

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

        public ITypeModel GetExpressionModel(ITypeSymbol type, IEnumerable<Expression> expressions)
        {
            Contract.Requires(this.AreSortsMatching(type, expressions));

            return new IntegerModel(this, type, (IntHandle)expressions.Single());
        }

        public IValueModel GetValueModel(ITypeSymbol type, IEnumerable<Interpretation> values)
        {
            Contract.Requires(this.AreSortsMatching(type, values));

            return new IntegerValueModel(this, type, (IntHandle)values.Single());
        }

        public IValueModel GetLiteralValueModel(ITypeSymbol type, object literalValue)
        {
            // TODO: Make it work more universally (also for ulong etc.)
            long value = Convert.ToInt64(literalValue);
            var interpretation = ExpressionFactory.IntInterpretation(value);

            return this.GetValueModel(type, interpretation.ToSingular());
        }

        // TODO: Somehow handle the different sizes and signed/unsigned variants using modulo etc.
        public void ModelOperation(IModellingContext context, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            Contract.Requires(context != null);
            Contract.Requires(method != null);
            Contract.Requires(arguments != null);
            Contract.Requires(arguments.Count() == method.Parameters.Length);

            if (method.MethodKind != MethodKind.BuiltinOperator
                || method.Parameters.Length == 0
                || (!BooleanModelFactory.Instance.IsTypeSupported(method.ReturnType)
                    && !this.IsTypeSupported(method.ReturnType)))
            {
                // This might be the case of conversions to other types, static methods etc.
                context.SetUnsupported();
                return;
            }

            IntHandle intResult;
            BoolHandle boolResult;
            this.GetOperationResult(context, method, arguments, out intResult, out boolResult);

            ITypeModel resultModel = null;

            if (intResult.Expression != null)
            {
                Contract.Assert(this.IsTypeSupported(method.ReturnType));
                resultModel = new IntegerModel(this, method.ReturnType, intResult);
            }
            else if (boolResult.Expression != null)
            {
                Contract.Assert(BooleanModelFactory.Instance.IsTypeSupported(method.ReturnType));

                // TODO: Properly manage the references to the other factories instead of using singletons for all types
                resultModel = BooleanModelFactory.Instance.GetExpressionModel(
                    method.ReturnType,
                    boolResult.Expression.ToSingular());
            }

            if (resultModel != null)
            {
                context.SetResultValue(resultModel);
            }
            else
            {
                context.SetUnsupported();
            }
        }

        private void GetOperationResult(
            IModellingContext context,
            IMethodSymbol method,
            IEnumerable<ITypeModel> arguments,
            out IntHandle intResult,
            out BoolHandle boolResult)
        {
            var first = ((IntegerModel)arguments.First()).Value;

            if (method.Parameters.Length == 1)
            {
                if (method.Name == "op_UnaryNegation")
                {
                    intResult = -first;
                }
                else if (method.Name == "op_UnaryPlus")
                {
                    intResult = first;
                }
            }
            else
            {
                Contract.Assert(method.Parameters.Length == 2);
                var second = ((IntegerModel)arguments.ElementAt(1)).Value;

                if (BooleanModelFactory.Instance.IsTypeSupported(method.ReturnType))
                {
                    switch (method.Name)
                    {
                        case "op_Equality":
                            boolResult = (first == second);
                            break;
                        case "op_Inequality":
                            boolResult = (first != second);
                            break;
                        case "op_GreaterThan":
                            boolResult = (first > second);
                            break;
                        case "op_LessThan":
                            boolResult = (first < second);
                            break;
                        case "op_GreaterThanOrEqual":
                            boolResult = (first >= second);
                            break;
                        case "op_LessThanOrEqual":
                            boolResult = (first <= second);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    Contract.Assert(this.IsTypeSupported(method.ReturnType));

                    switch (method.Name)
                    {
                        case "op_Addition":
                            intResult = first + second;
                            break;
                        case "op_Subtraction":
                            intResult = first - second;
                            break;
                        case "op_Division":
                            // TODO: Set the exception if the right is zero
                            intResult = first / second;
                            break;
                        case "op_Modulus":
                            intResult = first % second;
                            break;
                        case "op_Multiply":
                            intResult = first * second;
                            break;
                        default:
                            break;
                    }
                }
            }
        }
    }
}
