using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IExplorationHeuristic : IHeuristic
    {
        ExplorationState PickNextNode();

        IEnumerable<bool> DoBranch(ExplorationState node, IReadOnlyList<FlowEdge> edges);
    }
}
