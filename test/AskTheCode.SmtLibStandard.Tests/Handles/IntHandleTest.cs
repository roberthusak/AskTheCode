using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.SmtLibStandard.Handles.Tests
{
    [TestClass]
    public class IntHandleTest
    {
        private IntHandle a;
        private IntHandle b;
        private IntHandle c;

        public IntHandleTest()
        {
            this.a = (IntHandle)ExpressionFactory.NamedVariable(Sort.Int, "a");
            this.b = (IntHandle)ExpressionFactory.NamedVariable(Sort.Int, "b");
            this.c = (IntHandle)ExpressionFactory.NamedVariable(Sort.Int, "c");
        }

        // TODO: Test the interpretations construction

        [TestMethod]
        public void NegateOperatorConstructedProperly()
        {
            var negateA = -a;

            ExpressionTestHelper.CheckExpressionWithChildren(
                negateA.Expression,
                ExpressionKind.Negate,
                Sort.Int,
                "(- a)",
                a.Expression);
        }

        [TestMethod]
        public void MultiplyOperatorConstructedProperly()
        {
            var aMultiplyB = a * b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aMultiplyB.Expression,
                ExpressionKind.Multiply,
                Sort.Int,
                "(* a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void MultiplyOperatorMergedProperly()
        {
            var nultiplyABC = a * b * c;

            ExpressionTestHelper.CheckExpressionWithChildren(
                nultiplyABC.Expression,
                ExpressionKind.Multiply,
                Sort.Int,
                "(* a b c)",
                a.Expression,
                b.Expression,
                c.Expression);
        }

        [TestMethod]
        public void DivideRealOperatorConstructedProperly()
        {
            var aDivideRealB = a.DivideReal(b);

            ExpressionTestHelper.CheckExpressionWithChildren(
                aDivideRealB.Expression,
                ExpressionKind.DivideReal,
                Sort.Real,
                "(/ a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void DivideIntegerOperatorConstructedProperly()
        {
            var aDivideIntegerB = a / b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aDivideIntegerB.Expression,
                ExpressionKind.DivideInteger,
                Sort.Int,
                "(div a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void ModulusOperatorConstructedProperly()
        {
            var aModuloB = a % b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aModuloB.Expression,
                ExpressionKind.Modulus,
                Sort.Int,
                "(mod a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void RemainderOperatorConstructedProperly()
        {
            var aRemainderB = a.Remainder(b);

            ExpressionTestHelper.CheckExpressionWithChildren(
                aRemainderB.Expression,
                ExpressionKind.Remainder,
                Sort.Int,
                "(rem a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void AddOperatorConstructedProperly()
        {
            var aAddB = a + b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aAddB.Expression,
                ExpressionKind.Add,
                Sort.Int,
                "(+ a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void AddOperatorMergedProperly()
        {
            var addABC = a + b + c;

            ExpressionTestHelper.CheckExpressionWithChildren(
                addABC.Expression,
                ExpressionKind.Add,
                Sort.Int,
                "(+ a b c)",
                a.Expression,
                b.Expression,
                c.Expression);
        }

        [TestMethod]
        public void SubtractOperatorConstructedProperly()
        {
            var aSubtractB = a - b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aSubtractB.Expression,
                ExpressionKind.Subtract,
                Sort.Int,
                "(- a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void LessThanOperatorConstructedProperly()
        {
            var aLtB = a < b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aLtB.Expression,
                ExpressionKind.LessThan,
                Sort.Bool,
                "(< a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void GreaterThanOperatorConstructedProperly()
        {
            var aGtB = a > b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aGtB.Expression,
                ExpressionKind.GreaterThan,
                Sort.Bool,
                "(> a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void LessThanOrEqualOperatorConstructedProperly()
        {
            var aLtB = a <= b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aLtB.Expression,
                ExpressionKind.LessThanOrEqual,
                Sort.Bool,
                "(<= a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void GreaterThanOrEqualOperatorConstructedProperly()
        {
            var aGteB = a >= b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aGteB.Expression,
                ExpressionKind.GreaterThanOrEqual,
                Sort.Bool,
                "(>= a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void EqualOperatorConstructedProperly()
        {
            var aEqB = (a == b);

            ExpressionTestHelper.CheckExpressionWithChildren(
                aEqB.Expression,
                ExpressionKind.Equal,
                Sort.Bool,
                "(= a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void DistinctOperatorConstructedProperly()
        {
            var aNeqB = (a != b);

            ExpressionTestHelper.CheckExpressionWithChildren(
                aNeqB.Expression,
                ExpressionKind.Distinct,
                Sort.Bool,
                "(distinct a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void IfThenElseOperatorConstructedProperly()
        {
            var cond = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "cond");

            var ifCondThenAElseB = cond.IfThenElse(a, b);

            ExpressionTestHelper.CheckExpressionWithChildren(
                ifCondThenAElseB.Expression,
                ExpressionKind.IfThenElse,
                Sort.Int,
                "(ite cond a b)",
                cond.Expression,
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void NestedOperatorsWorkProperly()
        {
            var nested = (a == b) || (b >= a + a * c + a / b);

            var or = nested.Expression;

            ExpressionTestHelper.CheckExpression(
                or,
                ExpressionKind.Or,
                Sort.Bool,
                "(or (= a b) (>= b (+ a (* a c) (div a b))))",
                2);

            var eq = or.Children.ElementAt(0);

            ExpressionTestHelper.CheckExpressionWithChildren(
                eq,
                ExpressionKind.Equal,
                Sort.Bool,
                "(= a b)",
                a.Expression,
                b.Expression);

            var geq = or.Children.ElementAt(1);

            ExpressionTestHelper.CheckExpression(
                geq,
                ExpressionKind.GreaterThanOrEqual,
                Sort.Bool,
                "(>= b (+ a (* a c) (div a b)))",
                2);
            Assert.AreEqual(b.Expression, geq.Children.ElementAt(0));

            var add = geq.Children.ElementAt(1);

            ExpressionTestHelper.CheckExpression(
                add,
                ExpressionKind.Add,
                Sort.Int,
                "(+ a (* a c) (div a b))",
                3);
            Assert.AreEqual(a.Expression, add.Children.ElementAt(0));

            var mul = add.Children.ElementAt(1);

            ExpressionTestHelper.CheckExpressionWithChildren(
                mul,
                ExpressionKind.Multiply,
                Sort.Int,
                "(* a c)",
                a.Expression,
                c.Expression);

            var div = add.Children.ElementAt(2);

            ExpressionTestHelper.CheckExpressionWithChildren(
                div,
                ExpressionKind.DivideInteger,
                Sort.Int,
                "(div a b)",
                a.Expression,
                b.Expression);
        }
    }
}
