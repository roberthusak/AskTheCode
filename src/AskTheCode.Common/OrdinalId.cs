using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Common
{
    // TODO: Try to use Code Contracts to discourage the usage of default constructor (or at least in the comments)
    public struct OrdinalId : IOrdinalId<OrdinalId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public OrdinalId(int value)
        {
            this.value = value;

#if DEBUG
            this.isValid = true;
#endif
        }

        public bool IsValid
        {
            get
            {
#if DEBUG
                return this.isValid;
#else
                return true;
#endif
            }
        }

        public int Value
        {
            get { return this.value; }
        }

        public bool Equals(OrdinalId other)
        {
            return this.Value == other.Value;
        }
    }
}
