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

        public IEnumerable<bool> DoReuse(SmtSolverHandler solverHandler, IReadOnlyList<ExplorationNode> branchedNodes)
        {
            foreach (var node in branchedNodes)
            {
                yield return true;
            }
        }

        public bool DoSolve(ExplorationNode node)
        {
            return true;
        }

        public SmtSolverHandler SelectMergedSolverHandler(ExplorationNode current, ExplorationNode existing)
        {
            return current.SolverHandler;
        }
    }
}
