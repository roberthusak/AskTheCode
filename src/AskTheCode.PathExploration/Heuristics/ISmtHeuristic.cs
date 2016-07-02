using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface ISmtHeuristic : IHeuristic
    {
        bool DoSolve(ExplorationNode node);

        IEnumerable<bool> DoReuse(SmtSolverHandler solverHandler, IReadOnlyList<ExplorationNode> branchedNodes);

        SmtSolverHandler SelectMergedSolverHandler(ExplorationNode current, ExplorationNode existing);
    }
}
