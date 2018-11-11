using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    public static class PathExtensions
    {
        public static IEnumerable<FlowNode> Nodes(this Path path)
        {
            Contract.Requires(path != null);

            var p = path;
            do
            {
                yield return p.Node;
                p = p.Preceeding.IsEmpty ? null : p.Preceeding[0];
            }
            while (p != null);
        }
    }
}
