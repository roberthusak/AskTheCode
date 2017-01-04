using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <summary>
    /// Structure that mimics the behaviour of an immutable list with one element.
    /// </summary>
    public struct Singular<T> : IReadOnlyList<T>
    {
        public readonly T Value;

        public Singular(T value)
        {
            this.Value = value;
        }

        public int Count
        {
            get { return 1; }
        }

        public T this[int index]
        {
            get
            {
                if (index != 0)
                {
                    throw new IndexOutOfRangeException();
                }

                return this.Value;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            yield return this.Value;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this.Value;
        }
    }

    /// <summary>
    /// Helper static methods for <see cref="Singular{T}"/>.
    /// </summary>
    public static class Singular
    {
        public static Singular<T> Create<T>(T value)
        {
            return new Singular<T>(value);
        }

        public static Singular<T> ToSingular<T>(this T value)
        {
            return new Singular<T>(value);
        }
    }
}
