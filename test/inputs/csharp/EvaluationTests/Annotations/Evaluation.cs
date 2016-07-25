using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Pex.Framework;

namespace EvaluationTests.Annotations
{
    public static class Evaluation
    {
        public const string ContractsHintsSymbol = "EVALUATION_CC_HINTS";

        public static void ValidAssert(bool condition)
        {
            Contract.Requires(condition);
        }

        public static void InvalidAssert(bool condition)
        {
            Contract.Requires(condition);
        }

        public static void ValidUnreachable()
        {
            Contract.Requires(false);
        }

        public static void InvalidUnreachable()
        {
            Contract.Requires(false);
        }

        public static T Choose<T>(string name)
        {
            return PexChoose.Value<T>(name);
        }
    }
}
