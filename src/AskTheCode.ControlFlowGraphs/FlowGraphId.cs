using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public struct FlowGraphId : IOrdinalId<FlowGraphId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public FlowGraphId(int value)
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

        public bool Equals(FlowGraphId other)
        {
            return this.Value == other.Value;
        }

        public override string ToString() => this.Value.ToString();

        public class Provider : IIdProvider<FlowGraphId>
        {
            private OrdinalIdValueGenerator valueGenerator = new OrdinalIdValueGenerator();

            public FlowGraphId GenerateNewId()
            {
                int id = this.valueGenerator.GenerateNextIdValue();
                return new FlowGraphId(id);
            }
        }
    }
}
