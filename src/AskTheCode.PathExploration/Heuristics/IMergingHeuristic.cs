using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IMergingHeuristic : IHeuristic
    {
        bool DoExpectMerging(ExplorationNode node);

        bool DoMerge(ExplorationNode current, ExplorationNode existing);
    }
}
