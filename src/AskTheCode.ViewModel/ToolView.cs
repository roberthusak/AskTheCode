using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.PathExploration;
using AskTheCode.SmtLibStandard.Z3;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    public class ToolView : NotifyPropertyChangedBase
    {
        private readonly IIdeServices ideServices;

        private FlowGraphView selectedFlowGraph;
        private bool isExploring;
        private CancellationTokenSource explorationCancelSource;
        private PathView selectedPath;

        public ToolView(IIdeServices ideServices)
        {
            Contract.Requires<ArgumentNullException>(ideServices != null, nameof(ideServices));

            this.ideServices = ideServices;
            this.DisplayFlowGraphCommand = new Command(this.DisplayFlowGraph);
            this.ExploreCommand = new Command(this.Explore);
            this.CancelCommand = new Command(this.Cancel);

            this.UpdateCurrentSolution();
        }

        public ObservableCollection<FlowGraphView> FlowGraphs { get; private set; } =
            new ObservableCollection<FlowGraphView>();

        public FlowGraphView SelectedFlowGraph
        {
            get { return this.selectedFlowGraph; }
            set { this.SetProperty(ref this.selectedFlowGraph, value); }
        }

        public bool IsExploring
        {
            get { return this.isExploring; }
            private set { this.SetProperty(ref this.isExploring, value); }
        }

        public ObservableCollection<PathView> Paths { get; private set; } =
            new ObservableCollection<PathView>();

        public PathView SelectedPath
        {
            get { return this.selectedPath; }
            set { this.SetProperty(ref this.selectedPath, value); }
        }

        public Command DisplayFlowGraphCommand { get; private set; }

        public Command ExploreCommand { get; private set; }

        public Command CancelCommand { get; private set; }

        // TODO: Implement highlighting centrally and remove this
        internal IIdeServices IdeServices => this.ideServices;

        internal Solution CurrentSolution { get; private set; }

        internal CSharpFlowGraphProvider GraphProvider { get; private set; }

        public async void DisplayFlowGraph()
        {
            this.UpdateCurrentSolution();
            var info = await this.GatherInformationForCurrentCaretPosition();
            if (!info.IsComplete)
            {
                return;
            }

            var graphView = new FlowGraphView(info.Document, info.Location, info.FlowGraph, info.DisplayGraph);
            this.FlowGraphs.Add(graphView);
            this.SelectedFlowGraph = graphView;
        }

        public async void Explore()
        {
            if (this.IsExploring)
            {
                return;
            }

            this.UpdateCurrentSolution();
            var info = await this.GatherInformationForCurrentCaretPosition();
            if (!info.IsComplete)
            {
                return;
            }

            this.SelectedPath = null;
            this.Paths.Clear();

            Contract.Assert(info.SelectedDisplayNode.Records.Any());
            var flowNodeRecord = info.SelectedDisplayNode.Records.Last();

            var startNode = new StartingNodeInfo(
                flowNodeRecord.FlowNode,
                flowNodeRecord.FirstVariableIndex,
                info.IsAssertion);
            var z3ContextFactory = new ContextFactory();
            var options = new ExplorationOptions();
            options.FinalNodeRecognizer = new PublicMethodEntryRecognizer();
            var explorationContext = new ExplorationContext(this.GraphProvider, z3ContextFactory, startNode, options);

            this.explorationCancelSource = new CancellationTokenSource();

            // TODO: Solve the situation when there is no dispatcher associated with the current thread
            explorationContext.ExecutionModels
                .ObserveOn(DispatcherScheduler.Current)
                .Subscribe(this.OnExecutionModelFound, this.explorationCancelSource.Token);

            this.IsExploring = true;
            await explorationContext.ExploreAsync(this.explorationCancelSource);

            if (this.explorationCancelSource != null)
            {
                // To remove the subscription
                this.explorationCancelSource.Cancel();
                this.explorationCancelSource = null;
            }

            this.IsExploring = false;
        }

        public void Cancel()
        {
            if (!this.IsExploring)
            {
                return;
            }

            this.explorationCancelSource.Cancel();
            this.explorationCancelSource = null;
            this.IsExploring = false;
        }

        private void OnExecutionModelFound(ExecutionModel executionModel)
        {
            var pathView = new PathView(this, executionModel);
            this.Paths.Add(pathView);
        }

        private void UpdateCurrentSolution()
        {
            if (this.CurrentSolution == this.ideServices.Workspace.CurrentSolution)
            {
                return;
            }

            this.CurrentSolution = this.ideServices.Workspace.CurrentSolution;
            this.GraphProvider = new CSharpFlowGraphProvider(this.CurrentSolution);

            // TODO: Recreate or invalidate all the associated views (CFGs, execution model etc.
        }

        private async Task<CaretPositionInformation> GatherInformationForCurrentCaretPosition()
        {
            var info = new CaretPositionInformation();

            if (!this.ideServices.TryGetCaretPosition(out info.Document, out info.Position))
            {
                return info;
            }

            var root = await info.Document.GetSyntaxRootAsync();
            if (root == null)
            {
                return info;
            }

            var semanticModel = await info.Document.GetSemanticModelAsync();
            var caretChildNode = root.FindNode(new TextSpan(info.Position, 0));
            var methodDeclaration = caretChildNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (semanticModel == null || methodDeclaration == null)
            {
                return info;
            }

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
            {
                return info;
            }

            // TODO: Polish the usage of this thing
            info.Location = new MethodLocation(methodSymbol);
            info.FlowGraph = await this.GraphProvider.GetFlowGraphAsync(info.Location);
            info.DisplayGraph = this.GraphProvider.GetDisplayGraph(info.FlowGraph.Id);

            foreach (var displayNode in info.DisplayGraph.Nodes)
            {
                if (displayNode.Span.Contains(info.Position))
                {
                    info.SelectedDisplayNode = displayNode;
                    break;
                }
            }

            if (info.SelectedDisplayNode != null)
            {
                info.IsComplete = true;

                var syntaxNode = root.FindNode(info.SelectedDisplayNode.Span);
                if (syntaxNode != null)
                {
                    var selectedMethodSymbol = semanticModel.GetSymbolInfo(syntaxNode).Symbol as IMethodSymbol;
                    if (selectedMethodSymbol != null)
                    {
                        info.IsAssertion = AssertionMethodRecognizer.IsAssertionMethod(selectedMethodSymbol);
                    }
                }
            }

            return info;
        }

        private class CaretPositionInformation
        {
            public bool IsComplete;

            public Document Document;
            public int Position;
            public MethodLocation Location;
            public FlowGraph FlowGraph;
            public DisplayGraph DisplayGraph;
            public DisplayNode SelectedDisplayNode;
            public bool IsAssertion;
        }
    }
}
