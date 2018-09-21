using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public abstract class SimpleSmtHeuristic : ISmtHeuristic
    {
        public void Initialize(Explorer explorer)
        {
        }

        public IEnumerable<bool> DoReuse(SmtSolverHandler solverHandler, IReadOnlyList<ExplorationState> branchedStates)
        {
            foreach (var state in branchedStates)
            {
                yield return true;
            }
        }

        public abstract bool DoSolve(ExplorationState state);

        public SmtSolverHandler SelectMergedSolverHandler(ExplorationState current, ExplorationState existing)
        {
            return current.SolverHandler;
        }
    }
}
