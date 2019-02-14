using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.PathExploration;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.Msagl.Drawing;

namespace AskTheCode.ViewModel
{
    public class CallGraphView : NotifyPropertyChangedBase, IGraphViewerConsumer
    {
        private static readonly Color FoundCounterexampleColor = new Color(255, 148, 148);

        private static readonly Color UnreachableColor = new Color(155, 255, 99);

        private static readonly Color UnknownColor = Color.White;

        private readonly ToolView toolView;

        private IViewer graphViewer;

        public CallGraphView(ToolView toolView)
        {
            this.toolView = toolView;
        }

        public IViewer GraphViewer
        {
            get { return this.graphViewer; }
            set { this.SetProperty(ref this.graphViewer, value); }
        }

        public async void Redraw()
        {
            if (this.GraphViewer == null || this.toolView.ExplorationContext == null)
            {
                return;
            }

            var graph = new Graph();
            graph.Attr.LayerDirection = LayerDirection.TB;

            var visitedIds = new HashSet<int>();
            var context = this.toolView.ExplorationContext;
            var graphProvider = this.toolView.GraphProvider;

            // Handle the starting node
            var startCfg = context.StartingNode.Node.Graph;
            var startMethod = graphProvider.GetLocation(startCfg.Id);
            var startNode = graph.AddNode(startCfg.Id.ToString());
            startNode.Label.Text = startMethod.ToString();
            startNode.Label.FontColor = Color.White;
            startNode.Attr.FillColor = Color.Black;
            visitedIds.Add(startCfg.Id.Value);

            foreach (var exec in context.ExecutionModels)
            {
                await AddMethodNodesFromFlow(graph, visitedIds, graphProvider, exec.PathNodes, FoundCounterexampleColor);
            }

            foreach (var state in context.Explorer.States)
            {
                await AddMethodNodesFromFlow(graph, visitedIds, graphProvider, state.Path.Nodes(), UnknownColor);
            }

            foreach (int cfgId in visitedIds)
            {
                var methodSymbol = graphProvider.GetLocation(new FlowGraphId(cfgId)).Method;
                var callers = await SymbolFinder.FindCallersAsync(methodSymbol, this.toolView.CurrentSolution);
                foreach (var caller in callers)
                {
                    if (caller.CallingSymbol is IMethodSymbol callerMethod)
                    {
                        var callerCfg = await graphProvider.GetFlowGraphAsync(new MethodLocation(callerMethod));
                        if (visitedIds.Contains(callerCfg.Id.Value))
                        {
                            graph.AddEdge(callerCfg.Id.ToString(), cfgId.ToString());
                        }
                        else if (cfgId == startCfg.Id.Value)
                        {
                            // Safe caller of the target
                            var node = graph.AddNode(callerCfg.Id.ToString());
                            node.LabelText = new MethodLocation(callerMethod).ToString();
                            node.Attr.FillColor = UnreachableColor;

                            var edge = graph.AddEdge(callerCfg.Id.ToString(), cfgId.ToString());
                            edge.Attr.AddStyle(Style.Dashed);
                        }
                    }
                }
            }

            this.GraphViewer.Graph = graph;
        }

        protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
            if (propertyName == nameof(this.GraphViewer))
            {
                this.Redraw();
            }
        }

        private static async Task AddMethodNodesFromFlow(
            Graph graph,
            HashSet<int> visitedIds,
            CSharpFlowGraphProvider graphProvider,
            IEnumerable<FlowNode> nodes,
            Color nodeColor)
        {
            // TODO: Make more robust (unfinished calls from the target etc.)
            foreach (var callNode in nodes.OfType<CallFlowNode>())
            {
                var cfgId = callNode.Graph.Id;
                if (visitedIds.Add(cfgId.Value))
                {
                    var location = graphProvider.GetLocation(cfgId);

                    var node = graph.AddNode(cfgId.Value.ToString());
                    node.LabelText = location.ToString();
                    node.Attr.FillColor = nodeColor;
                }

                if (callNode.Location.CanBeExplored
                    && await graphProvider.GetFlowGraphAsync(callNode.Location) is FlowGraph trg
                    && visitedIds.Add(trg.Id.Value))
                {
                    var node = graph.AddNode(trg.Id.ToString());
                    node.LabelText = callNode.Location.ToString();
                    node.Attr.FillColor = nodeColor;
                }
            }
        }
    }
}
