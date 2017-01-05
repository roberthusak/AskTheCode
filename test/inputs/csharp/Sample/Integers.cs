using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public static class Integers
    {
        public static int IntAdd(int a, int b)
        {
            return a + b;
        }

        public static int IntMax(int a, int b)
        {
            if (a > b)
            {
                return a;
            }
            else
            {
                return b;
            }
        }

        public static int IntAddNested(int a, int b, int c)
        {
            a = b + c + c;
            int d = b;
            b = c + IntAdd(a, b);
            a = b = c + IntAdd(a, b);
            d = d + d;
            int e = b, f;
            e = d;

            return IntAdd(a + c, b) + c;
        }

        public static int IntOperationsExample(int a, int b, int c)
        {
            int d = a + b;
            d = -a;
            d = -(a + c);
            d = a - c;
            d = a / c;
            d = a % c;
            d = a * c;

            d = a * b + c / 2;
            d = a * (b + c) / 2;
            return d;
        }
    }
}
