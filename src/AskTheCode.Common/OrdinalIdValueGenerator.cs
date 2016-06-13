using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    /// <remarks>
    /// This class is thread safe.
    /// </remarks>
    public class OrdinalIdValueGenerator
    {
        private int nextValue;

        public OrdinalIdValueGenerator(int startValue = 0)
        {
            this.nextValue = startValue;
        }

        public int GenerateNextIdValue()
        {
            int next = Interlocked.Increment(ref this.nextValue);
            return next - 1;
        }
    }
}
