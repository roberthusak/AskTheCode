using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration.Heuristics
{
    public class LimitedExplorationHeuristic : IExplorationHeuristic
    {
        private Explorer explorer;

        public LimitedExplorationHeuristic(int loopLimit, int recursionLimit)
        {
            this.LoopLimit = loopLimit;
            this.RecursionLimit = recursionLimit;
        }

        public int LoopLimit { get; }

        public int RecursionLimit { get; }

        public void Initialize(Explorer explorer)
        {
            this.explorer = explorer;
        }

        public IEnumerable<bool> DoBranch(ExplorationState state, IReadOnlyList<FlowEdge> edges)
        {
            foreach (var edge in edges)
            {
                if (edge is OuterFlowEdge outerEdge)
                {
                    // What routine are we interested in
                    var routineGraph = outerEdge.From.Graph;

                    // Determine call/return balance of the given routine
                    int balance = (outerEdge.Kind == OuterFlowEdgeKind.MethodCall) ? 1 : -1;
                    foreach (var pathEdge in state.Path.Edges().OfType<OuterFlowEdge>())
                    {
                        if (pathEdge.To.Graph == routineGraph && pathEdge.Kind == OuterFlowEdgeKind.MethodCall)
                        {
                            balance++;
                        }
                        else if (pathEdge.From.Graph == routineGraph && pathEdge.Kind == OuterFlowEdgeKind.Return)
                        {
                            balance--;
                        }
                    }

                    // Do not let the actual call stack (its subset with this routine) height grow over the limit
                    yield return Math.Abs(balance) <= this.RecursionLimit;
                }
                else if (edge.From.Flags.HasFlag(FlowNodeFlags.LoopBody))
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
                else
                {
                    yield return true;
                }
            }
        }

        public ExplorationState PickNextState()
        {
            // TODO: Favor leaving the loops as early as possible
            return this.explorer.States.FirstOrDefault();
        }
    }

    public class LimitedExplorationHeuristicFactory : IHeuristicFactory<LimitedExplorationHeuristic>
    {
        public LimitedExplorationHeuristicFactory(int loopLimit, int recursionLimit)
        {
            this.LoopLimit = loopLimit;
            this.RecursionLimit = recursionLimit;
        }

        public int LoopLimit { get; }

        public int RecursionLimit { get; }

        public LimitedExplorationHeuristic CreateHeuristic(Explorer explorer)
        {
            var heuristic = new LimitedExplorationHeuristic(this.LoopLimit, this.RecursionLimit);
            heuristic.Initialize(explorer);

            return heuristic;
        }
    }
}
