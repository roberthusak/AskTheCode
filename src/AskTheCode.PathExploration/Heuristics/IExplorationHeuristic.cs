using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IExplorationHeuristic : IHeuristic
    {
        ExplorationNode PickNextNode();

        IEnumerable<bool> DoBranch(ExplorationNode node, IReadOnlyList<FlowEdge> edges);
    }
}
