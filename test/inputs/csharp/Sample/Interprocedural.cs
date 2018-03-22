using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public static class Interprocedural
    {
        public static void SimpleCaller(int x, int y)
        {
            int a = x + y;
            int z = SimpleCallee(a);
            int b = -a;
            int c = b;
        }

        public static void OtherSimpleCaller(int x, int y)
        {
            int a = 1;
            int c = x - y;
            int b = SimpleCallee(a);
        }

        private static int SimpleCallee(int y)
        {
            int a = 11;
            return -y;
        }

        public static void BranchedCaller(int x, int y)
        {
            int a = x + y;
            int c = BranchedCallee(x);
            int b = -a;

            if (c == 0)
            {
                int d = c;
                Debug.Assert(c != 0);
                return;
            }
            else
            {
                return;
            }
        }

        private static int BranchedCallee(int x)
        {
            if (x == 0)
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }
    }
}
