using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public class SimpleHeuristicFactory<THeuristic> : IHeuristicFactory<THeuristic>
        where THeuristic : IHeuristic, new()
    {
        public THeuristic CreateHeuristic(Explorer explorer)
        {
            var heuristic = new THeuristic();
            heuristic.Initialize(explorer);

            return heuristic;
        }
    }
}
