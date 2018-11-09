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
    }
}
