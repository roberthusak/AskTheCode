using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs
{
    public struct InnerFlowEdgeId : IOrdinalId<InnerFlowEdgeId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public InnerFlowEdgeId(int value)
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

        public bool Equals(InnerFlowEdgeId other)
        {
            return this.Value == other.Value;
        }

        internal class Provider : IIdProvider<InnerFlowEdgeId>
        {
            private OrdinalIdValueGenerator valueGenerator = new OrdinalIdValueGenerator();

            public InnerFlowEdgeId GenerateNewId()
            {
                int id = this.valueGenerator.GenerateNextIdValue();
                return new InnerFlowEdgeId(id);
            }
        }
    }
}
