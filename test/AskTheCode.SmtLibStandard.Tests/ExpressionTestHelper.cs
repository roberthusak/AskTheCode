using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.SmtLibStandard.Tests
{
    public static class ExpressionTestHelper
    {
        public static void CheckExpression(
            Expression expression,
            ExpressionKind kind,
            Sort sort,
            string stringValue,
            int? childrenCount = null)
        {
            Assert.AreNotEqual(null, expression);
            Assert.AreEqual(kind, expression.Kind);
            Assert.AreEqual(sort, expression.Sort);
            Assert.AreEqual(stringValue, expression.ToString());

            // TODO: Consider making it required
            if (childrenCount != null)
            {
                Assert.AreEqual(childrenCount, expression.ChildrenCount);
            }
        }

        public static void CheckExpressionWithChildren(
            Expression expression,
            ExpressionKind kind,
            Sort sort,
            string stringValue,
            params Expression[] children)
        {
            CheckExpression(expression, kind, sort, stringValue, children.Length);

            for (int i = 0; i < children.Length; i++)
            {
                Assert.AreEqual(children[i], expression.Children.ElementAt(i));
            }
        }
    }
}
