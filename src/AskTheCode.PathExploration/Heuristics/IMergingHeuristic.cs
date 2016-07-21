using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IMergingHeuristic : IHeuristic
    {
        bool DoExpectMerging(ExplorationState node);

        bool DoMerge(ExplorationState current, ExplorationState existing);
    }
}
