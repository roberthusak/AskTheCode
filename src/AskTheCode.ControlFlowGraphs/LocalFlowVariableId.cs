using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public struct LocalFlowVariableId : IOrdinalId<LocalFlowVariableId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public LocalFlowVariableId(int value)
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

        public bool Equals(LocalFlowVariableId other)
        {
            return this.Value == other.Value;
        }

        internal class Provider : IIdProvider<LocalFlowVariableId>
        {
            private OrdinalIdValueGenerator valueGenerator = new OrdinalIdValueGenerator();

            public LocalFlowVariableId GenerateNewId()
            {
                int id = this.valueGenerator.GenerateNextIdValue();
                return new LocalFlowVariableId(id);
            }
        }
    }
}
