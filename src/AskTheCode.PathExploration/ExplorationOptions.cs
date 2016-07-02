using AskTheCode.PathExploration.Heuristics;

namespace AskTheCode.PathExploration
{
    public class ExplorationOptions
    {
        public static readonly ExplorationOptions Default;

        static ExplorationOptions()
        {
            Default = new ExplorationOptions()
            {
                FinalNodeRecognizer = new BorderFinalNodeRecognizer(),
                ExplorationHeuristicFactory = new SimpleHeuristicFactory<GreedyExplorationHeuristic>(),
                MergingHeuristicFactory = new SimpleHeuristicFactory<NeverMergeHeuristic>(),
                SmtHeuristicFactory = new SimpleHeuristicFactory<SimpleSmtHeuristic>()
            };
        }

        public ExplorationOptions()
        {
        }

        public IFinalNodeRecognizer FinalNodeRecognizer { get; set; }

        public IHeuristicFactory<IExplorationHeuristic> ExplorationHeuristicFactory { get; set; }

        public IHeuristicFactory<IMergingHeuristic> MergingHeuristicFactory { get; set; }

        public IHeuristicFactory<ISmtHeuristic> SmtHeuristicFactory { get; set; }
    }
}