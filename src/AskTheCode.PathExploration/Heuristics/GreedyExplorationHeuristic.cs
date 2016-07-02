using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration.Heuristics
{
    public class GreedyExplorationHeuristic : IExplorationHeuristic
    {
        private Explorer explorer;

        public void Initialize(Explorer explorer)
        {
            this.explorer = explorer;
        }

        public IEnumerable<bool> DoBranch(ExplorationNode node, IReadOnlyList<FlowEdge> edges)
        {
            foreach (var edge in edges)
            {
                yield return true;
            }
        }

        public ExplorationNode PickNextNode()
        {
            return this.explorer.Nodes.First();
        }
    }
}
