using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Heap
{
    public class ReferenceComparisonVariable : LocalFlowVariable
    {
        internal ReferenceComparisonVariable(
            FlowGraph graph,
            LocalFlowVariableId id,
            bool areEqual,
            FlowVariable left,
            FlowVariable right)
            : base(graph, id, Sort.Bool, GetDisplayName(id, areEqual, left, right))
        {
            Contract.Requires(left != null);
            Contract.Requires(right != null);
            Contract.Requires(left.IsReference);
            Contract.Requires(right.IsReference);

            this.AreEqual = areEqual;
            this.Left = left;
            this.Right = right;
        }

        public bool AreEqual { get; }

        public FlowVariable Left { get; }

        public FlowVariable Right { get; }

        public static implicit operator BoolHandle(ReferenceComparisonVariable comparison)
        {
            return new BoolHandle(comparison);
        }

        private static string GetDisplayName(
            LocalFlowVariableId id,
            bool areEqual,
            FlowVariable left,
            FlowVariable right)
        {
            return $"{left}_{(areEqual ? "eq" : "neq")}_{right}!{id.Value}";
        }
    }
}
