using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public static class Combinations
    {
        public static bool IntBoolOperationsExample(int a, int b, bool c)
        {
            bool d = (a == b);
            d = (a != b);
            d = (a > b);
            d = (a < b);
            d = (a >= b);
            d = (a <= b);

            d = ((a > b) || c);

            if ((a > b) && (a != b || c))
            {
                return c;
            }

            return !c;
        }

        public static int LogicExample(int a, int b, int c, bool condition)
        {
            if ((a == 0 || 10 / b > 3) && (c == 3 || (5 / b > 1 && condition)))
            {
                return a;
            }
            else if (condition == false)
            {
                return b;
            }
            else
            {
                return c;
            }
        }

        public static int ComplexExample(int a, int b, bool c)
        {
            if (a == 0)
            {
                return -b;
            }

            if (IsNiceNumber(a) && a / b > 2 && b != -1)
            {
                int d = b;
                while (a < b)
                {
                    a += a;
                    Debug.Assert(a != 0);
                }
            }
            else
            {
                a += b;
            }

            return a;
        }

        public static bool IsNiceNumber(int i)
        {
            if (i % 2 == 0)
            {
                return true;
            }
            else if (i % 3 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static void DataIndependentCycle(int x)
        {
            int i = 0;
            while (i < 20)
            {
                x = 2 * x;
                i = i + 1;
            }

            Debug.Assert(x < 32);
        }
    }
}
