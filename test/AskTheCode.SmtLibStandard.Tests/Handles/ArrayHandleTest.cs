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
    public class ArrayHandleTest
    {
        private static Sort arraySort = Sort.GetArray(Sort.Int, Sort.Bool);

        private ArrayHandle<IntHandle, BoolHandle> a;
        private IntHandle k;
        private BoolHandle v;

        public ArrayHandleTest()
        {
            this.a = (ArrayHandle<IntHandle, BoolHandle>)ExpressionFactory.NamedVariable(arraySort, "a");
            this.k = (IntHandle)ExpressionFactory.NamedVariable(Sort.Int, "k");
            this.v = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "v");
        }

        [TestMethod]
        public void SelectConstructedProperly()
        {
            var selectAK = a.Select(k);

            ExpressionTestHelper.CheckExpressionWithChildren(
                selectAK.Expression,
                ExpressionKind.Select,
                Sort.Bool,
                "(select a k)",
                a.Expression,
                k.Expression);
        }

        [TestMethod]
        public void StoreConstructedProperly()
        {
            var storeAKV = a.Store(k, v);

            ExpressionTestHelper.CheckExpressionWithChildren(
                storeAKV.Expression,
                ExpressionKind.Store,
                arraySort,
                "(store a k v)",
                a.Expression,
                k.Expression,
                v.Expression);
        }

        [TestMethod]
        public void NestedSelectStoreConstructedProperly()
        {
            var nested = a.Store(k, v).Select(k);

            ExpressionTestHelper.CheckExpression(
                nested.Expression,
                ExpressionKind.Select,
                Sort.Bool,
                "(select (store a k v) k)",
                2);

            var storeAKV = nested.Expression.Children.ElementAt(0);

            ExpressionTestHelper.CheckExpressionWithChildren(
                storeAKV,
                ExpressionKind.Store,
                arraySort,
                "(store a k v)",
                a.Expression,
                k.Expression,
                v.Expression);

            var kExpr = nested.Expression.Children.ElementAt(1);

            Assert.AreEqual(k, kExpr);
        }
    }
}
