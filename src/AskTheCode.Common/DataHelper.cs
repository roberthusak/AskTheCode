using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public static class DataHelper
    {
        public static void SetOnceAssert<T>(ref T storage, T value)
        {
            Contract.Assert(object.Equals(storage, default(T)));

            storage = value;
        }

        public static void SetOnceException<T>(ref T storage, T value)
        {
            if (!object.Equals(storage, default(T)))
            {
                throw new InvalidOperationException();
            }

            storage = value;
        }

        public static void Swap<T>(ref T a, ref T b)
        {
            T helper = a;
            a = b;
            b = helper;
        }
    }
}
