using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public struct GlobalFlowVariableId : IOrdinalId<GlobalFlowVariableId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public GlobalFlowVariableId(int value)
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

        public bool Equals(GlobalFlowVariableId other)
        {
            return this.Value == other.Value;
        }

        // TODO: Should we change the type to something to be hashed (it might be dense)? String?
        public class Provider : IIdProvider<GlobalFlowVariableId>
        {
            private OrdinalIdValueGenerator valueGenerator = new OrdinalIdValueGenerator();

            public GlobalFlowVariableId GenerateNewId()
            {
                int id = this.valueGenerator.GenerateNextIdValue();
                return new GlobalFlowVariableId(id);
            }
        }
    }
}
