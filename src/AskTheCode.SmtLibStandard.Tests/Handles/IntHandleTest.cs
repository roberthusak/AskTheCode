using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        [TestMethod]
        public void NegateOperatorConstructedProperly()
        {
            var negateA = -a;

            Assert.AreNotEqual(null, negateA.Expression);
            Assert.AreEqual(ExpressionKind.Negate, negateA.Expression.Kind);
            Assert.AreEqual(Sort.Int, negateA.Expression.Sort);
            Assert.AreEqual(1, negateA.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, negateA.Expression.Children.ElementAt(0));
            Assert.AreEqual("(- a)", negateA.ToString());
        }

        [TestMethod]
        public void MultiplyOperatorConstructedProperly()
        {
            var aMultiplyB = a * b;

            Assert.AreNotEqual(null, aMultiplyB.Expression);
            Assert.AreEqual(ExpressionKind.Multiply, aMultiplyB.Expression.Kind);
            Assert.AreEqual(Sort.Int, aMultiplyB.Expression.Sort);
            Assert.AreEqual(2, aMultiplyB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aMultiplyB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aMultiplyB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(* a b)", aMultiplyB.ToString());
        }

        [TestMethod]
        public void DivideRealOperatorConstructedProperly()
        {
            var aDivideRealB = a.DivideReal(b);

            Assert.AreNotEqual(null, aDivideRealB.Expression);
            Assert.AreEqual(ExpressionKind.DivideReal, aDivideRealB.Expression.Kind);
            Assert.AreEqual(Sort.Real, aDivideRealB.Expression.Sort);
            Assert.AreEqual(2, aDivideRealB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aDivideRealB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aDivideRealB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(/ a b)", aDivideRealB.ToString());
        }

        [TestMethod]
        public void DivideIntegerOperatorConstructedProperly()
        {
            var aDivideIntegerB = a / b;

            Assert.AreNotEqual(null, aDivideIntegerB.Expression);
            Assert.AreEqual(ExpressionKind.DivideInteger, aDivideIntegerB.Expression.Kind);
            Assert.AreEqual(Sort.Int, aDivideIntegerB.Expression.Sort);
            Assert.AreEqual(2, aDivideIntegerB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aDivideIntegerB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aDivideIntegerB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(div a b)", aDivideIntegerB.ToString());
        }

        [TestMethod]
        public void ModulusOperatorConstructedProperly()
        {
            var aModuloB = a % b;

            Assert.AreNotEqual(null, aModuloB.Expression);
            Assert.AreEqual(ExpressionKind.Modulus, aModuloB.Expression.Kind);
            Assert.AreEqual(Sort.Int, aModuloB.Expression.Sort);
            Assert.AreEqual(2, aModuloB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aModuloB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aModuloB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(mod a b)", aModuloB.ToString());
        }

        [TestMethod]
        public void RemainderOperatorConstructedProperly()
        {
            var aRemainderB = a.Remainder(b);

            Assert.AreNotEqual(null, aRemainderB.Expression);
            Assert.AreEqual(ExpressionKind.Remainder, aRemainderB.Expression.Kind);
            Assert.AreEqual(Sort.Int, aRemainderB.Expression.Sort);
            Assert.AreEqual(2, aRemainderB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aRemainderB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aRemainderB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(rem a b)", aRemainderB.ToString());
        }

        [TestMethod]
        public void AddOperatorConstructedProperly()
        {
            var aAddB = a + b;

            Assert.AreNotEqual(null, aAddB.Expression);
            Assert.AreEqual(ExpressionKind.Add, aAddB.Expression.Kind);
            Assert.AreEqual(Sort.Int, aAddB.Expression.Sort);
            Assert.AreEqual(2, aAddB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aAddB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aAddB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(+ a b)", aAddB.ToString());
        }

        [TestMethod]
        public void SubtractOperatorConstructedProperly()
        {
            var aSubtractB = a - b;

            Assert.AreNotEqual(null, aSubtractB.Expression);
            Assert.AreEqual(ExpressionKind.Subtract, aSubtractB.Expression.Kind);
            Assert.AreEqual(Sort.Int, aSubtractB.Expression.Sort);
            Assert.AreEqual(2, aSubtractB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aSubtractB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aSubtractB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(- a b)", aSubtractB.ToString());
        }

        [TestMethod]
        public void LessThanOperatorConstructedProperly()
        {
            var aLtB = a < b;

            Assert.AreNotEqual(null, aLtB.Expression);
            Assert.AreEqual(ExpressionKind.LessThan, aLtB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aLtB.Expression.Sort);
            Assert.AreEqual(2, aLtB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aLtB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aLtB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(< a b)", aLtB.ToString());
        }

        [TestMethod]
        public void GreaterThanOperatorConstructedProperly()
        {
            var aGtB = a > b;

            Assert.AreNotEqual(null, aGtB.Expression);
            Assert.AreEqual(ExpressionKind.GreaterThan, aGtB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aGtB.Expression.Sort);
            Assert.AreEqual(2, aGtB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aGtB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aGtB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(> a b)", aGtB.ToString());
        }

        [TestMethod]
        public void LessThanOrEqualOperatorConstructedProperly()
        {
            var aLtB = a <= b;

            Assert.AreNotEqual(null, aLtB.Expression);
            Assert.AreEqual(ExpressionKind.LessThanOrEqual, aLtB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aLtB.Expression.Sort);
            Assert.AreEqual(2, aLtB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aLtB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aLtB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(<= a b)", aLtB.ToString());
        }

        [TestMethod]
        public void GreaterThanOrEqualOperatorConstructedProperly()
        {
            var aGteB = a >= b;

            Assert.AreNotEqual(null, aGteB.Expression);
            Assert.AreEqual(ExpressionKind.GreaterThanOrEqual, aGteB.Expression.Kind);
            Assert.AreEqual(Sort.Bool, aGteB.Expression.Sort);
            Assert.AreEqual(2, aGteB.Expression.ChildrenCount);
            Assert.AreEqual(a.Expression, aGteB.Expression.Children.ElementAt(0));
            Assert.AreEqual(b.Expression, aGteB.Expression.Children.ElementAt(1));
            Assert.AreEqual("(>= a b)", aGteB.ToString());
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
            var cond = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "cond");

            var ifCondThenAElseB = cond.IfThenElse(a, b);

            Assert.AreNotEqual(null, ifCondThenAElseB.Expression);
            Assert.AreEqual(ExpressionKind.IfThenElse, ifCondThenAElseB.Expression.Kind);
            Assert.AreEqual(Sort.Int, ifCondThenAElseB.Expression.Sort);
            Assert.AreEqual(3, ifCondThenAElseB.Expression.ChildrenCount);
            Assert.AreEqual(cond.Expression, ifCondThenAElseB.Expression.Children.ElementAt(0));
            Assert.AreEqual(a.Expression, ifCondThenAElseB.Expression.Children.ElementAt(1));
            Assert.AreEqual(b.Expression, ifCondThenAElseB.Expression.Children.ElementAt(2));
            Assert.AreEqual("(ite cond a b)", ifCondThenAElseB.ToString());
        }
    }
}
