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
    public class BoolHandleTest
    {
        private BoolHandle a;
        private BoolHandle b;
        private BoolHandle c;

        public BoolHandleTest()
        {
            this.a = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "a");
            this.b = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "b");
            this.c = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "c");
        }

        [TestMethod]
        public void InterpretationConstructedProperly()
        {
            var trueVal = new BoolHandle(true);

            ExpressionTestHelper.CheckExpression(
                trueVal.Expression,
                ExpressionKind.Interpretation,
                Sort.Bool,
                true.ToString(),
                0);
            Assert.AreEqual(ExpressionFactory.True, trueVal.Expression);

            BoolHandle falseVal = false;

            ExpressionTestHelper.CheckExpression(
                falseVal.Expression,
                ExpressionKind.Interpretation,
                Sort.Bool,
                false.ToString(),
                0);
            Assert.AreEqual(ExpressionFactory.False, falseVal.Expression);
        }

        [TestMethod]
        public void NotOperatorConstructedProperly()
        {
            var notA = !a;

            ExpressionTestHelper.CheckExpressionWithChildren(
                notA.Expression,
                ExpressionKind.Not,
                Sort.Bool,
                "(not a)",
                a.Expression);
        }

        [TestMethod]
        public void AndOperatorConstructedProperly()
        {
            var aAndB = a && b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aAndB.Expression,
                ExpressionKind.And,
                Sort.Bool,
                "(and a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void AndOperatorMergedProperly()
        {
            var andABC = a && b && c;

            ExpressionTestHelper.CheckExpressionWithChildren(
                andABC.Expression,
                ExpressionKind.And,
                Sort.Bool,
                "(and a b c)",
                a.Expression,
                b.Expression,
                c.Expression);
        }

        [TestMethod]
        public void OrOperatorConstructedProperly()
        {
            var aOrB = a || b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aOrB.Expression,
                ExpressionKind.Or,
                Sort.Bool,
                "(or a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void OrOperatorMergedProperly()
        {
            var orABC = a || b || c;

            ExpressionTestHelper.CheckExpressionWithChildren(
                orABC.Expression,
                ExpressionKind.Or,
                Sort.Bool,
                "(or a b c)",
                a.Expression,
                b.Expression,
                c.Expression);
        }

        [TestMethod]
        public void XorOperatorConstructedProperly()
        {
            var aXorB = a ^ b;

            ExpressionTestHelper.CheckExpressionWithChildren(
                aXorB.Expression,
                ExpressionKind.Xor,
                Sort.Bool,
                "(xor a b)",
                a.Expression,
                b.Expression);
        }

        [TestMethod]
        public void ImpliesOperatorConstructedProperly()
        {
            var aImpliesB = a.Implies(b);

            ExpressionTestHelper.CheckExpressionWithChildren(
                aImpliesB.Expression,
                ExpressionKind.Implies,
                Sort.Bool,
                "(=> a b)",
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

            var f = aNeqB != c;
        }

        [TestMethod]
        public void DistinctOperatorNestedProperly()
        {
            var aNeqBNeqC = (a != b != c);

            ExpressionTestHelper.CheckExpression(
                aNeqBNeqC.Expression,
                ExpressionKind.Distinct,
                Sort.Bool,
                "(distinct (distinct a b) c)",
                2);

            var aNeqB = aNeqBNeqC.Expression.Children.ElementAt(0);

            ExpressionTestHelper.CheckExpressionWithChildren(
                aNeqB,
                ExpressionKind.Distinct,
                Sort.Bool,
                "(distinct a b)",
                a.Expression,
                b.Expression);

            Assert.AreEqual(c.Expression, aNeqBNeqC.Expression.Children.ElementAt(1));
        }

        [TestMethod]
        public void IfThenElseOperatorConstructedProperly()
        {
            var ifAThenBElseC = a.IfThenElse(b, c);

            ExpressionTestHelper.CheckExpressionWithChildren(
                ifAThenBElseC.Expression,
                ExpressionKind.IfThenElse,
                Sort.Bool,
                "(ite a b c)",
                a.Expression,
                b.Expression,
                c.Expression);
        }

        [TestMethod]
        public void NestedOperatorsWorkProperly()
        {
            var nested = (a && b && c) || (b == c) || b.Implies(c);

            var or = nested.Expression;

            ExpressionTestHelper.CheckExpression(
                or,
                ExpressionKind.Or,
                Sort.Bool,
                "(or (and a b c) (= b c) (=> b c))",
                3);

            var and = or.Children.ElementAt(0);

            ExpressionTestHelper.CheckExpressionWithChildren(
                and,
                ExpressionKind.And,
                Sort.Bool,
                "(and a b c)",
                a.Expression,
                b.Expression,
                c.Expression);

            var eq = or.Children.ElementAt(1);

            ExpressionTestHelper.CheckExpressionWithChildren(
                eq,
                ExpressionKind.Equal,
                Sort.Bool,
                "(= b c)",
                b.Expression,
                c.Expression);

            var implies = or.Children.ElementAt(2);

            ExpressionTestHelper.CheckExpressionWithChildren(
                implies,
                ExpressionKind.Implies,
                Sort.Bool,
                "(=> b c)",
                b.Expression,
                c.Expression);
        }
    }
}
