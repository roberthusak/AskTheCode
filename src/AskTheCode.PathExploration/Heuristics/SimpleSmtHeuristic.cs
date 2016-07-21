using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public class SimpleSmtHeuristic : ISmtHeuristic
    {
        public void Initialize(Explorer explorer)
        {
        }

        public IEnumerable<bool> DoReuse(SmtSolverHandler solverHandler, IReadOnlyList<ExplorationState> branchedNodes)
        {
            foreach (var node in branchedNodes)
            {
                yield return true;
            }
        }

        public bool DoSolve(ExplorationState node)
        {
            return true;
        }

        public SmtSolverHandler SelectMergedSolverHandler(ExplorationState current, ExplorationState existing)
        {
            return current.SolverHandler;
        }
    }
}
