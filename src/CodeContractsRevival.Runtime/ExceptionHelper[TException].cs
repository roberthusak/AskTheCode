using System;

namespace CodeContractsRevival.Runtime
{
    internal static class ExceptionHelper<TException>
        where TException : Exception
    {
        public static readonly Func<TException> Create = InitCreate();

        public static readonly Func<string, TException> CreateWithMessage = InitCreateWithMessage();

        private static Func<TException> InitCreate()
        {
            var constructor = typeof(TException).GetConstructor(Type.EmptyTypes);
            return () => (TException)constructor.Invoke(null);
        }

        private static Func<string, TException> InitCreateWithMessage()
        {
            var constructor = typeof(TException).GetConstructor(new Type[] { typeof(string) });
            return (message) => (TException)constructor.Invoke(new string[] { message });
        }
    }
}
