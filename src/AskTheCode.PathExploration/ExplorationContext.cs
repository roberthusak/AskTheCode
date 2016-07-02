using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.PathExploration.Heuristics;

namespace AskTheCode.PathExploration
{
    public class ExplorationContext
    {
        private List<ExecutionModel> executionModels = new List<ExecutionModel>();

        public ExplorationContext(
            IFlowGraphProvider flowGraphProvider,
            StartingNodeInfo startingNode,
            ExplorationOptions options)
        {
            this.FlowGraphProvider = flowGraphProvider;
            this.StartingNode = startingNode;
            this.Options = options;
        }

        public ExplorationContext(IFlowGraphProvider flowGraphProvider, StartingNodeInfo startingNode)
            : this(flowGraphProvider, startingNode, ExplorationOptions.Default)
        {
        }

        public event EventHandler<ExecutionModelEventArgs> ExecutionModelFound;

        public IReadOnlyCollection<ExecutionModel> ExecutionModels
        {
            get { return this.executionModels; }
        }

        internal IFlowGraphProvider FlowGraphProvider { get; private set; }

        internal IHeuristicFactory<IExplorationHeuristic> ExplorationHeuristicFactory { get; private set; }

        internal IHeuristicFactory<IMergingHeuristic> MergingHeuristicFactory { get; private set; }

        internal StartingNodeInfo StartingNode { get; private set; }

        internal ExplorationOptions Options { get; private set; }

        public void Explore()
        {
            var explorer = this.CreateExplorer();
            explorer.Explore();
        }

        public async Task ExploreAsync()
        {
            // TODO: Implement exploration partitioning to multiple explorers
            var explorer = this.CreateExplorer();

            var explorationTask = new Task(explorer.Explore);
            explorationTask.Start();

            await explorationTask;
        }

        private Explorer CreateExplorer()
        {
            var explorer = new Explorer(
                this,
                this.StartingNode,
                this.Options.FinalNodeRecognizer,
                this.ExplorerResultCallback);

            explorer.ExplorationHeuristic = this.Options.ExplorationHeuristicFactory.CreateHeuristic(explorer);
            explorer.MergingHeuristic = this.Options.MergingHeuristicFactory.CreateHeuristic(explorer);
            explorer.SmtHeuristic = this.Options.SmtHeuristicFactory.CreateHeuristic(explorer);

            return explorer;
        }

        private void ExplorerResultCallback(ExplorationResult result)
        {
            // TODO: Implement locking when multiple explorers are implemented
            if (result?.ExecutionModel != null)
            {
                this.executionModels.Add(result.ExecutionModel);

                this.ExecutionModelFound?.Invoke(this, new ExecutionModelEventArgs(result.ExecutionModel));
            }
            else if (result?.PathCounterExample != null)
            {
                // TODO
            }
        }
    }
}
