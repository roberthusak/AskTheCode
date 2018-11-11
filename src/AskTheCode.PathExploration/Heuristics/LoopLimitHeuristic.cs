using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration.Heuristics
{
    public class LoopLimitHeuristic : IExplorationHeuristic
    {
        private Explorer explorer;

        public LoopLimitHeuristic(int loopLimit)
        {
            this.LoopLimit = loopLimit;
        }

        public int LoopLimit { get; }

        public void Initialize(Explorer explorer)
        {
            this.explorer = explorer;
        }

        public IEnumerable<bool> DoBranch(ExplorationState state, IReadOnlyList<FlowEdge> edges)
        {
            foreach (var edge in edges)
            {
                if (!edge.From.Flags.HasFlag(FlowNodeFlags.LoopBody))
                {
                    yield return true;
                }
                else
                {
                    // TODO: Consider using subscribing to extensions and retractions instead
                    var loopCondNode = state.Path.Nodes().FirstOrDefault(n => n.Flags.HasFlag(FlowNodeFlags.LoopCondition));
                    if (loopCondNode == null)
                    {
                        yield return true;
                    }
                    else
                    {
                        // LoopLimit enters from iterations + 1 enter from the code after the loop
                        int loopCount = state.Path.Nodes().Count(n => n == loopCondNode);
                        yield return loopCount <= this.LoopLimit + 1;
                    }
                }
            }
        }

        public ExplorationState PickNextState()
        {
            // TODO: Favor leaving the loops as early as possible
            return this.explorer.States.FirstOrDefault();
        }
    }

    public class LoopLimitHeuristicFactory : IHeuristicFactory<LoopLimitHeuristic>
    {
        public LoopLimitHeuristicFactory(int loopLimit)
        {
            this.LoopLimit = loopLimit;
        }

        public int LoopLimit { get; }

        public LoopLimitHeuristic CreateHeuristic(Explorer explorer)
        {
            var heuristic = new LoopLimitHeuristic(this.LoopLimit);
            heuristic.Initialize(explorer);

            return heuristic;
        }
    }
}
