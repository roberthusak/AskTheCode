using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface ISmtHeuristic : IHeuristic
    {
        bool DoSolve(ExplorationState node);

        IEnumerable<bool> DoReuse(SmtSolverHandler solverHandler, IReadOnlyList<ExplorationState> branchedNodes);

        SmtSolverHandler SelectMergedSolverHandler(ExplorationState current, ExplorationState existing);
    }
}
