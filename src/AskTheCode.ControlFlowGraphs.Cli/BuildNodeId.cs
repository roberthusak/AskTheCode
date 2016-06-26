using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal struct BuildNodeId : IOrdinalId<BuildNodeId>
    {
        private readonly int value;

#if DEBUG
        private readonly bool isValid;
#endif

        public BuildNodeId(int value)
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

        public bool Equals(BuildNodeId other)
        {
            return this.Value == other.Value;
        }

        internal class Provider : IIdProvider<BuildNodeId>
        {
            private OrdinalIdValueGenerator valueGenerator = new OrdinalIdValueGenerator();

            public BuildNodeId GenerateNewId()
            {
                int id = this.valueGenerator.GenerateNextIdValue();
                return new BuildNodeId(id);
            }
        }
    }
}
