using AskTheCode.PathExploration.Heap;
using AskTheCode.PathExploration.Heuristics;

namespace AskTheCode.PathExploration
{
    public class ExplorationOptions
    {
        public static ExplorationOptions Default
        {
            get { return new ExplorationOptions(); }
        }

        public IEntryPointRecognizer FinalNodeRecognizer { get; set; } = new BorderEntryPointRecognizer();

        public ISymbolicHeapFactory SymbolicHeapFactory { get; set; } = new ProxySimplifyingSymbolicHeapFactory();

        public IHeuristicFactory<IExplorationHeuristic> ExplorationHeuristicFactory { get; set; } =
            new SimpleHeuristicFactory<GreedyExplorationHeuristic>();

        public IHeuristicFactory<IMergingHeuristic> MergingHeuristicFactory { get; set; } =
            new SimpleHeuristicFactory<NeverMergeHeuristic>();

        public IHeuristicFactory<ISmtHeuristic> SmtHeuristicFactory { get; set; } =
            new SimpleHeuristicFactory<SimpleSmtHeuristic>();

        public int? TimeoutSeconds { get; set; } = 30;
    }
}