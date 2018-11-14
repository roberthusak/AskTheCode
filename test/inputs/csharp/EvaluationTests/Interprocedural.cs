using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EvaluationTests.Annotations;

namespace EvaluationTests
{
    public static class Interprocedural
    {
        private static int MultiCallee(int x)
        {
            int a = x + 1;
            return a;
        }

        public static void MultiCaller()
        {
            int b = MultiCallee(1);
            int c = MultiCallee(2);

            Evaluation.InvalidUnreachable();
        }

        public static void MultiCaller2()
        {
            int b = MultiCallee(1);
            b = MultiCallee(2);

            Evaluation.InvalidUnreachable();
        }

        private static bool GreaterOrEqualRecursive(int a, int b)
        {
            if (a == b)
            {
                return true;
            }
            else if (a == 0)
            {
                return false;
            }
            else
            {
                return GreaterOrEqualRecursive(a - 1, b);
            }
        }

        public static void RecursiveTest()
        {
            bool c = GreaterOrEqualRecursive(3, 1);
            Evaluation.ValidAssert(c);
            if (c)
            {
                Evaluation.InvalidUnreachable();
            }
        }
    }
}
