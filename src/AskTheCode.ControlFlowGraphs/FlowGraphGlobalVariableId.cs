using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public struct FlowGraphGlobalVariableId : IOrdinalId<FlowGraphGlobalVariableId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public FlowGraphGlobalVariableId(int value)
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

        public bool Equals(FlowGraphGlobalVariableId other)
        {
            return this.Value == other.Value;
        }

        // TODO: Should we change the type to something to be hashed (it might be dense)? String?
        public class Provider : IIdProvider<FlowGraphGlobalVariableId>
        {
            private OrdinalIdValueGenerator valueGenerator = new OrdinalIdValueGenerator();

            public FlowGraphGlobalVariableId GenerateNewId()
            {
                int id = this.valueGenerator.GenerateNextIdValue();
                return new FlowGraphGlobalVariableId(id);
            }
        }
    }
}
