using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample
{
    public static class Boolean
    {
        public static bool BoolOperationsExample(bool a, bool b, bool c)
        {
            bool d = (a && b);
            d = (a & b);
            d = (a || b);
            d = (a | b);
            d = (a == b);
            d = (a != b);
            d = !c;

            d = a & b | c;
            d = a && b || c;
            d = a && (b || c);

            return a;
        }
    }
}
