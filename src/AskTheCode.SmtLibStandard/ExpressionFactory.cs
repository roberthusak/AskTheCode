using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace AskTheCode.SmtLibStandard
{
    public static class ExpressionFactory
    {
        static ExpressionFactory()
        {
            True = new Interpretation(Sort.Bool, true);
            False = new Interpretation(Sort.Bool, false);
        }

        public static Interpretation True { get; private set; }

        public static Interpretation False { get; private set; }

        public static Interpretation BoolInterpretation(bool value)
        {
            return value ? True : False;
        }

        public static Interpretation IntInterpretation(long value)
        {
            return new Interpretation(Sort.Int, value);
        }

        public static Interpretation RealInterpretation(double value)
        {
            return new Interpretation(Sort.Real, value);
        }

        public static NamedVariable NamedVariable(Sort sort, SymbolName name)
        {
            Contract.Requires(sort != null);
            Contract.Requires(name.IsValid);

            return new NamedVariable(sort, name);
        }

        public static NamedVariable NamedVariable(Sort sort, string nameText, int? nameNumber = null)
        {
            Contract.Requires(sort != null);

            var name = new SymbolName(nameText, nameNumber);
            return new NamedVariable(sort, name);
        }

        public static Function Not(Expression operand)
        {
            Contract.Requires(operand != null);
            Contract.Requires(operand.Sort == Sort.Bool);

            return UnaryFunction(ExpressionKind.Not, Sort.Bool, operand);
        }

        public static Function And(params Expression[] operands)
        {
            CheckBoolArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.And, Sort.Bool, operands, false);
        }

        public static Function AndNested(params Expression[] operands)
        {
            CheckBoolArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.And, Sort.Bool, operands, true);
        }

        public static Function Or(params Expression[] operands)
        {
            CheckBoolArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.Or, Sort.Bool, operands, false);
        }

        public static Function OrNested(params Expression[] operands)
        {
            CheckBoolArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.Or, Sort.Bool, operands, true);
        }

        public static Function Xor(Expression left, Expression right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Sort == Sort.Bool);
            Contract.Requires(right.Sort == Sort.Bool);

            return BinaryFunction(ExpressionKind.Xor, Sort.Bool, left, right);
        }

        public static Function Implies(Expression left, Expression right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Sort == Sort.Bool);
            Contract.Requires(right.Sort == Sort.Bool);

            return BinaryFunction(ExpressionKind.Implies, Sort.Bool, left, right);
        }

        public static Function Negate(Expression operand)
        {
            Contract.Requires(operand != null);
            Contract.Requires(operand.Sort.IsNumeric);

            return UnaryFunction(ExpressionKind.Negate, operand.Sort, operand);
        }

        public static Function Multiply(params Expression[] operands)
        {
            CheckNumericArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.Multiply, operands[0].Sort, operands, false);
        }

        public static Function MultiplyNested(params Expression[] operands)
        {
            CheckNumericArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.Multiply, operands[0].Sort, operands, true);
        }

        public static Function DivideReal(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.DivideReal, Sort.Real, left, right);
        }

        public static Function DivideInteger(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.DivideInteger, Sort.Int, left, right);
        }

        public static Function Modulus(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.Modulus, left.Sort, left, right);
        }

        public static Function Remainder(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.Remainder, left.Sort, left, right);
        }

        public static Function Add(params Expression[] operands)
        {
            CheckNumericArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.Add, operands[0].Sort, operands, false);
        }

        public static Function AddNested(params Expression[] operands)
        {
            CheckNumericArbitraryFunctionArguments(operands);

            return ArbitraryFunction(ExpressionKind.Add, operands[0].Sort, operands, true);
        }

        // TODO: Consider making it an arbitrary function as in Z3 (beware the limited associativity)
        public static Function Subtract(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.Subtract, left.Sort, left, right);
        }

        public static Function LessThan(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.LessThan, Sort.Bool, left, right);
        }

        public static Function GreaterThan(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.GreaterThan, Sort.Bool, left, right);
        }

        public static Function LessThanOrEqual(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.LessThanOrEqual, Sort.Bool, left, right);
        }

        public static Function GreaterThanOrEqual(Expression left, Expression right)
        {
            CheckNumericBinaryFunctionArguments(left, right);

            return BinaryFunction(ExpressionKind.GreaterThanOrEqual, Sort.Bool, left, right);
        }

        public static Function Equal(Expression left, Expression right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Sort == right.Sort);

            return BinaryFunction(ExpressionKind.Equal, Sort.Bool, left, right);
        }

        public static Function Distinct(params Expression[] operands)
        {
            Contract.Requires(operands != null);
            Contract.Requires(operands.Length > 1);
            Contract.Requires(Contract.ForAll(operands, operand => operand.Sort == operands[0].Sort));

            return ArbitraryFunction(ExpressionKind.Distinct, Sort.Bool, operands, true);
        }

        public static Function IfThenElse(Expression condition, Expression valueTrue, Expression valueFalse)
        {
            Contract.Requires(condition != null);
            Contract.Requires(valueTrue != null);
            Contract.Requires(valueFalse != null);
            Contract.Requires(condition.Sort == Sort.Bool);
            Contract.Requires(valueTrue.Sort == valueFalse.Sort);

            return TernaryFunction(ExpressionKind.IfThenElse, valueTrue.Sort, condition, valueTrue, valueFalse);
        }

        /// <remarks>
        /// Kind and sort validation is up to the caller.
        /// </remarks>
        internal static Function Function(ExpressionKind kind, Sort sort, Expression[] operands)
        {
            Contract.Requires(sort != null);
            Contract.Requires(operands != null);

            // TODO: Change to four different kinds (unary, binary etc.) if desirable
            // TODO: Implement proper operators merging (now it is defensively disabled)
            return ArbitraryFunction(kind, sort, operands, true);
        }

        [ContractAbbreviator]
        private static void CheckBoolArbitraryFunctionArguments(Expression[] operands)
        {
            Contract.Requires(operands != null);
            Contract.Requires(operands.Length > 1);
            Contract.Requires(Contract.ForAll(operands, operand => operand != null));
            Contract.Requires(Contract.ForAll(operands, operand => operand.Sort == Sort.Bool));
        }

        [ContractAbbreviator]
        private static void CheckNumericBinaryFunctionArguments(Expression left, Expression right)
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.Sort == right.Sort);
            Contract.Requires(left.Sort.IsNumeric);
        }

        [ContractAbbreviator]
        private static void CheckNumericArbitraryFunctionArguments(Expression[] operands)
        {
            Contract.Requires(operands != null);
            Contract.Requires(operands.Length > 1);
            Contract.Requires(Contract.ForAll(operands, operand => operand != null));
            Contract.Requires(Contract.ForAll(operands, operand => operand.Sort == operands[0].Sort));
            Contract.Requires(operands[0].Sort.IsNumeric);
        }

        /// <remarks>
        /// This method encapsulates the creation of functions with a certain number of arguments. That might be useful
        /// if we want to implement internal subclasses of <see cref="Function"/> such as UnaryFunction, BinaryFunction
        /// TernaryFunction and ArbitraryFunction. These might get used for the performance reasons - in order not to
        /// handle an array for every function (and just for the last variant).
        /// </remarks>
        private static Function UnaryFunction(ExpressionKind kind, Sort sort, Expression operand)
        {
            return new Function(kind, sort, operand);
        }

        /// <remarks>
        /// This method encapsulates the creation of functions with a certain number of arguments. That might be useful
        /// if we want to implement internal subclasses of <see cref="Function"/> such as UnaryFunction, BinaryFunction
        /// TernaryFunction and ArbitraryFunction. These might get used for the performance reasons - in order not to
        /// handle an array for every function (and just for the last variant).
        /// </remarks>
        private static Function BinaryFunction(ExpressionKind kind, Sort sort, Expression left, Expression right)
        {
            return new Function(kind, sort, left, right);
        }

        /// <remarks>
        /// This method encapsulates the creation of functions with a certain number of arguments. That might be useful
        /// if we want to implement internal subclasses of <see cref="Function"/> such as UnaryFunction, BinaryFunction
        /// TernaryFunction and ArbitraryFunction. These might get used for the performance reasons - in order not to
        /// handle an array for every function (and just for the last variant).
        /// </remarks>
        private static Function TernaryFunction(
            ExpressionKind kind,
            Sort sort,
            Expression first,
            Expression second,
            Expression third)
        {
            return new Function(kind, sort, first, second, third);
        }

        /// <remarks>
        /// This method encapsulates the creation of functions with a certain number of arguments. That might be useful
        /// if we want to implement internal subclasses of <see cref="Function"/> such as UnaryFunction, BinaryFunction
        /// TernaryFunction and ArbitraryFunction. These might get used for the performance reasons - in order not to
        /// handle an array for every function (and just for the last variant).
        /// </remarks>
        private static Function ArbitraryFunction(
            ExpressionKind kind,
            Sort sort,
            Expression[] operands,
            bool preserveNesting)
        {
            if (preserveNesting || !operands.Any(op => op.Kind == kind && op.Sort == sort))
            {
                return new Function(kind, sort, operands);
            }

            var mergedOperands = operands
                .SelectMany(op => (op.Kind == kind && op.Sort == sort) ? op.Children : Enumerable.Repeat(op, 1))
                .ToArray();
            return new Function(kind, sort, mergedOperands);
        }
    }
}
