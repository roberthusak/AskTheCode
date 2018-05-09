using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Heap
{
    /// <summary>
    /// Provides methods for reference handling in a CFG.
    /// </summary>
    public static class References
    {
        public static Sort Sort { get; } = Sort.CreateCustom("Reference");

        public static SpecialFlowVariable Null { get; } = new SpecialFlowVariable("null", Sort);

        public static bool IsReferenceComparison(
            Expression expr,
            out bool areEqual,
            out FlowVariable left,
            out FlowVariable right)
        {
            if ((expr?.Kind == ExpressionKind.Equal || expr?.Kind == ExpressionKind.Distinct)
                && expr.GetChild(0).Sort == Sort
                && expr.GetChild(0) is FlowVariable leftVar
                && expr.GetChild(1) is FlowVariable rightVar)
            {
                Contract.Assert(expr.Sort == Sort.Bool);
                Contract.Assert(expr.GetChild(1).Sort == Sort);

                areEqual = (expr.Kind == ExpressionKind.Equal);
                left = leftVar;
                right = rightVar;

                return true;
            }
            else
            {
                areEqual = false;
                left = null;
                right = null;

                return false;
            }
        }
    }
}
