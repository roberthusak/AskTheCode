using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public sealed class MultipleIngoingSmtHeuristic : SimpleSmtHeuristic
    {
        public override bool DoSolve(ExplorationState state)
        {
            return state.Path.Node.IngoingEdges.Count > 1;
        }
    }
}
