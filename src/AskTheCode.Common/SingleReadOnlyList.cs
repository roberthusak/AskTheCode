using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    public struct SingleReadOnlyList<T> : IReadOnlyList<T>
    {
        public readonly T Value;

        public SingleReadOnlyList(T value)
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
}
