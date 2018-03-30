using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    public class CallSiteStack : IEquatable<CallSiteStack>
    {
        private CallSiteStack(CallFlowNode callSite, CallSiteStack rest, int count)
        {
            this.Rest = rest;
            this.CallSite = callSite;
            this.Count = count;
        }

        public static CallSiteStack Empty { get; private set; } = new CallSiteStack(null, null, 0);

        public CallSiteStack Rest { get; private set; }

        public CallFlowNode CallSite { get; private set; }

        public int Count { get; private set; }

        public bool IsEmpty => this.Count == 0;

        public CallSiteStack Push(CallFlowNode callSite) => new CallSiteStack(callSite, this, this.Count + 1);

        public CallSiteStack Pop() => this.Rest;

        public bool Equals(CallSiteStack other)
        {
            if (other.Count != this.Count)
            {
                return false;
            }

            var otherRest = other.Rest;
            var selfRest = this.Rest;
            while (!otherRest.IsEmpty)
            {
                if (otherRest.CallSite != selfRest.CallSite)
                {
                    return false;
                }
            }

            Contract.Assert(otherRest == Empty);
            Contract.Assert(selfRest.IsEmpty);
            Contract.Assert(selfRest == Empty);
            return true;
        }
    }
}
