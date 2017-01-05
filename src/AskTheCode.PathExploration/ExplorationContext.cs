using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.PathExploration.Heuristics;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.PathExploration
{
    public class ExplorationContext
    {
        // TODO: Consider enforcing it to be read-only
        private Subject<ExecutionModel> executionModels = new Subject<ExecutionModel>();

        public ExplorationContext(
            IFlowGraphProvider flowGraphProvider,
            IContextFactory smtContextFactory,
            StartingNodeInfo startingNode,
            ExplorationOptions options)
        {
            this.FlowGraphProvider = flowGraphProvider;
            this.SmtContextFactory = smtContextFactory;
            this.StartingNode = startingNode;
            this.Options = options;
            this.Options.FinalNodeRecognizer.FlowGraphProvider = flowGraphProvider;
        }

        public ExplorationContext(
            IFlowGraphProvider flowGraphProvider,
            IContextFactory smtContextFactory,
            StartingNodeInfo startingNode)
            : this(flowGraphProvider, smtContextFactory, startingNode, ExplorationOptions.Default)
        {
        }

        public IObservable<ExecutionModel> ExecutionModels => this.executionModels;

        internal IFlowGraphProvider FlowGraphProvider { get; private set; }

        internal IContextFactory SmtContextFactory { get; private set; }

        internal IHeuristicFactory<IExplorationHeuristic> ExplorationHeuristicFactory { get; private set; }

        internal IHeuristicFactory<IMergingHeuristic> MergingHeuristicFactory { get; private set; }

        internal StartingNodeInfo StartingNode { get; private set; }

        internal ExplorationOptions Options { get; private set; }

        public async Task<bool> ExploreAsync(CancellationToken cancelToken)
        {
            Contract.Requires<InvalidOperationException>(this.Options.TimeoutSeconds == null);

            return await this.ExploreAsyncImpl(cancelToken);
        }

        public async Task<bool> ExploreAsync(CancellationTokenSource cancelTokenSource = null)
        {
            var cancelToken = cancelTokenSource?.Token ?? default(CancellationToken);

            if (cancelTokenSource == null && this.Options.TimeoutSeconds != null)
            {
                cancelTokenSource = new CancellationTokenSource();
                cancelToken = cancelTokenSource.Token;
            }

            if (this.Options.TimeoutSeconds != null)
            {
                cancelTokenSource.CancelAfter(this.Options.TimeoutSeconds.Value * 1000);
            }

            return await this.ExploreAsyncImpl(cancelToken);
        }

        /// <summary>
        /// Creates an instance of <see cref="Explorer"/> and runs the exploration on the ThreadPool.
        /// </summary>
        /// <remarks>
        /// In the future, it is expected to create multiple explorers and distribute the work among them.
        /// </remarks>
        private async Task<bool> ExploreAsyncImpl(CancellationToken cancelToken)
        {
            var explorer = this.CreateExplorer();
            return await Task.Run(() => explorer.ExploreAsync(cancelToken));
        }

        private Explorer CreateExplorer()
        {
            var explorer = new Explorer(
                this,
                this.SmtContextFactory,
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
            // TODO: Implement locking or some intelligent sequencing when multiple explorers are implemented
            if (result?.ExecutionModel != null)
            {
                this.executionModels.OnNext(result.ExecutionModel);
            }
            else if (result?.PathCounterExample != null)
            {
                // TODO
            }
        }
    }
}
