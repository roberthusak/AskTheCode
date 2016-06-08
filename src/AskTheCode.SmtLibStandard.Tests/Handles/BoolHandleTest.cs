using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public void NotOperatorConstructedProperly()
        {
            var notA = !a;

            Assert.AreNotEqual(null, notA.Expression);
            Assert.AreEqual(ExpressionKind.Not, notA.Expression.Kind);
            Assert.AreEqual(Sort.Bool, notA.Expression.Sort);
            Assert.AreEqual(1, notA.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, notA.Expression.Children.ElementAt(0));
            Assert.AreEqual("(not a)", notA.ToString());
        }

        [TestMethod]
        public void AndOperatorConstructedProperly()
        {
            var aAndB = a && b;

            Assert.AreNotEqual(null, aAndB.Expression);
            Assert.AreEqual(ExpressionKind.And, aAndB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aAndB.Expression.Sort);
            Assert.AreEqual(2, aAndB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aAndB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aAndB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(and a b)", aAndB.ToString());
        }

        [TestMethod]
        public void OrOperatorConstructedProperly()
        {
            var aOrB = a || b;

            Assert.AreNotEqual(null, aOrB.Expression);
            Assert.AreEqual(ExpressionKind.Or, aOrB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aOrB.Expression.Sort);
            Assert.AreEqual(2, aOrB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aOrB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aOrB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(or a b)", aOrB.ToString());
        }

        [TestMethod]
        public void XorOperatorConstructedProperly()
        {
            var aXorB = a ^ b;

            Assert.AreNotEqual(null, aXorB.Expression);
            Assert.AreEqual(ExpressionKind.Xor, aXorB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aXorB.Expression.Sort);
            Assert.AreEqual(2, aXorB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aXorB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aXorB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(xor a b)", aXorB.ToString());
        }

        [TestMethod]
        public void ImpliesOperatorConstructedProperly()
        {
            var aImpliesB = a.Implies(b);

            Assert.AreNotEqual(null, aImpliesB.Expression);
            Assert.AreEqual(ExpressionKind.Implies, aImpliesB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aImpliesB.Expression.Sort);
            Assert.AreEqual(2, aImpliesB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aImpliesB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aImpliesB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(=> a b)", aImpliesB.ToString());
        }

        [TestMethod]
        public void EqualOperatorConstructedProperly()
        {
            var aEqB = (a == b);

            Assert.AreNotEqual(null, aEqB.Expression);
            Assert.AreEqual(ExpressionKind.Equal, aEqB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aEqB.Expression.Sort);
            Assert.AreEqual(2, aEqB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aEqB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aEqB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(= a b)", aEqB.ToString());
        }

        [TestMethod]
        public void DistinctOperatorConstructedProperly()
        {
            var aNeqB = (a != b);

            Assert.AreNotEqual(null, aNeqB.Expression);
            Assert.AreEqual(ExpressionKind.Distinct, aNeqB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aNeqB.Expression.Sort);
            Assert.AreEqual(2, aNeqB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aNeqB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aNeqB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(distinct a b)", aNeqB.ToString());
        }

        [TestMethod]
        public void IfThenElseOperatorConstructedProperly()
        {
            var ifAThenBElseC = a.IfThenElse(b, c);

            Assert.AreNotEqual(null, ifAThenBElseC.Expression);
            Assert.AreEqual(ExpressionKind.IfThenElse, ifAThenBElseC.Expression.Kind);
            Assert.AreEqual(Sort.Bool, ifAThenBElseC.Expression.Sort);
            Assert.AreEqual(3, ifAThenBElseC.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, ifAThenBElseC.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, ifAThenBElseC.Expression.Children.ElementAt(1));
            Assert.AreEqual(c.Expression, ifAThenBElseC.Expression.Children.ElementAt(2));
            Assert.AreEqual("(ite a b c)", ifAThenBElseC.ToString());
        }

        [TestMethod]
        public void NestedOperatorsWorkProperly()
        {
            var nested = (a && b && c) | (b == c) | b.Implies(c);
            Assert.AreNotEqual(null, nested.Expression);

            var or = nested.Expression;
            Assert.AreEqual(ExpressionKind.Or, or.Kind);
            Assert.AreEqual(Sort.Bool, or.Sort);
            Assert.AreEqual(3, or.ChildrenCount);

            var and = or.Children.ElementAt(0);

            Assert.AreNotEqual(null, and);
            Assert.AreEqual(ExpressionKind.And, and.Kind);
            Assert.AreEqual(Sort.Bool, and.Sort);
            Assert.AreEqual(3, and.ChildrenCount);
            Assert.AreEqual(a.Expression, and.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, and.Children.ElementAt(1));
            Assert.AreEqual(c.Expression, and.Children.ElementAt(2));

            var eq = or.Children.ElementAt(1);

            Assert.AreNotEqual(null, eq);
            Assert.AreEqual(ExpressionKind.Equal, eq.Kind);
            Assert.AreEqual(Sort.Bool, eq.Sort);
            Assert.AreEqual(2, eq.ChildrenCount);
            Assert.AreEqual(b.Expression, eq.Children.ElementAt(0));
            Assert.AreEqual(c.Expression, eq.Children.ElementAt(1));

            var implies = or.Children.ElementAt(2);

            Assert.AreNotEqual(null, implies);
            Assert.AreEqual(ExpressionKind.Implies, implies.Kind);
            Assert.AreEqual(Sort.Bool, implies.Sort);
            Assert.AreEqual(2, implies.ChildrenCount);
            Assert.AreEqual(b.Expression, implies.Children.ElementAt(0));
            Assert.AreEqual(c.Expression, implies.Children.ElementAt(1));

            Assert.AreEqual("(or (and a b c) (= b c) (=> b c))", nested.ToString());
        }
    }
}
