using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IHeuristicFactory<out THeuristic>
        where THeuristic : IHeuristic
    {
        THeuristic CreateHeuristic(Explorer explorer);
    }
}
