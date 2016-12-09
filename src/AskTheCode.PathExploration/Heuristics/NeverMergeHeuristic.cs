using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public class NeverMergeHeuristic : IMergingHeuristic
    {
        public void Initialize(Explorer explorer)
        {
        }

        public bool DoExpectMerging(ExplorationState state)
        {
            return false;
        }

        public bool DoMerge(ExplorationState current, ExplorationState existing)
        {
            return false;
        }
    }
}
