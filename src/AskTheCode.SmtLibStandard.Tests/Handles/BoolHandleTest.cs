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
        [TestMethod]
        public void TestMethod()
        {
            var a = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "a");
            var b = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "b");
            var c = (BoolHandle)ExpressionFactory.NamedVariable(Sort.Bool, "c");

            var notA = !a;
            var aAndB = a & b;
            var aOrB = a | b;
            var aXorB = a ^ b;
            var aEqB = (a == b);
            var aNeqB = (a != b);
            var aImpliesB = a.Implies(b);
            var ifAThenBElseC = a.IfThenElse(b, c);

            var nested = (a & b) | (b == c) | b.Implies(c);
        }
    }
}
