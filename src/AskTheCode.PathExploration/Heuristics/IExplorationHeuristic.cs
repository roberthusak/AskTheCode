using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IExplorationHeuristic : IHeuristic
    {
        ExplorationState PickNextState();

        IEnumerable<bool> DoBranch(ExplorationState state, IReadOnlyList<FlowEdge> edges);
    }
}
