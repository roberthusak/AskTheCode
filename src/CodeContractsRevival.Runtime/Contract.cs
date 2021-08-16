using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace CodeContractsRevival.Runtime
{
    public static class Contract
    {
        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            Debug.Assert(condition);
        }

        [Conditional("DEBUG")]
        public static void Requires(bool condition)
        {
            Debug.Assert(condition);
        }

        [Conditional("DEBUG")]
        public static void Requires(bool condition, string userMessage)
        {
            Debug.Assert(condition, userMessage);
        }

        public static void Requires<TException>(bool condition) where TException : Exception
        {
            if (!condition)
            {
                throw ExceptionHelper<TException>.Create();
            }
        }

        public static void Requires<TException>(bool condition, string userMessage) where TException : Exception
        {
            if (!condition)
            {
                throw ExceptionHelper<TException>.CreateWithMessage(userMessage);
            }
        }

        public static bool ForAll<T>(IEnumerable<T> collection, Func<T, bool> predicate)
        {
            return collection.All(predicate);
        }
    }
}
