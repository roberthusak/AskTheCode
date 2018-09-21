using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public sealed class EntryPointSmtHeuristic : SimpleSmtHeuristic
    {
        public override bool DoSolve(ExplorationState state)
        {
            return false;
        }
    }
}
