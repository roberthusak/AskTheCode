using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.PathExploration.Heuristics
{
    public sealed class FixedIntervalSmtHeuristic : SimpleSmtHeuristic
    {
        private readonly int interval;

        public FixedIntervalSmtHeuristic(int interval)
        {
            this.interval = interval;
        }

        public override bool DoSolve(ExplorationState state)
        {
            return state.Path.Depth % this.interval == 0;
        }
    }

    public class FixedIntervalSmtHeuristicFactory : IHeuristicFactory<FixedIntervalSmtHeuristic>
    {
        private readonly int interval;

        public FixedIntervalSmtHeuristicFactory(int interval)
        {
            this.interval = interval;
        }

        public FixedIntervalSmtHeuristic CreateHeuristic(Explorer explorer)
        {
            return new FixedIntervalSmtHeuristic(this.interval);
        }
    }
}
