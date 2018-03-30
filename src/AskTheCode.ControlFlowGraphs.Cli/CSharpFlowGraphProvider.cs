using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    // TODO: Think out the multithreading - whether to lock the helper classes etc.
    public class CSharpFlowGraphProvider : IFlowGraphProvider
    {
        private FlowGraphId.Provider graphIdProvider = new FlowGraphId.Provider();
        private OrdinalOverlay<FlowGraphId, FlowGraph, GeneratedGraphs> generatedGraphs =
            new OrdinalOverlay<FlowGraphId, FlowGraph, GeneratedGraphs>();

        private Dictionary<IMethodSymbol, FlowGraphId> symbolsToGraphIdMap =
            new Dictionary<IMethodSymbol, FlowGraphId>();

        public CSharpFlowGraphProvider(Solution solution)
        {
            Contract.Requires<ArgumentNullException>(solution != null, nameof(solution));

            this.Solution = solution;
        }

        public TypeModelManager ModelManager { get; private set; } = new TypeModelManager();

        public Solution Solution { get; private set; }

        public FlowGraph this[FlowGraphId graphId]
        {
            get { return this.generatedGraphs[graphId].FlowGraph; }
        }

        public DisplayGraph GetDisplayGraph(FlowGraphId graphId)
        {
            return this.generatedGraphs[graphId].DisplayGraph;
        }

        public MethodLocation GetLocation(FlowGraphId graphId)
        {
            return this.generatedGraphs[graphId].Location;
        }

        IRoutineLocation IFlowGraphProvider.GetLocation(FlowGraphId graphId) => this.GetLocation(graphId);

        public async Task<FlowGraph> GetFlowGraphAsync(IRoutineLocation location)
        {
            Contract.Requires<ArgumentException>(location is MethodLocation, nameof(location));

            var graphs = await this.LazyGenerateGraphsAsync((MethodLocation)location);

            return graphs.FlowGraph;
        }

        public async Task<DisplayGraph> GetDisplayGraphAsync(IRoutineLocation location)
        {
            Contract.Requires<ArgumentException>(location is MethodLocation, nameof(location));

            var graphs = await this.LazyGenerateGraphsAsync((MethodLocation)location);

            return graphs.DisplayGraph;
        }

        public OuterFlowEdge GetCallEdge(CallFlowNode callNode, EnterFlowNode enterNode)
        {
            // TODO: Perform the proper comparison
            ////Contract.Requires(callNode.Location.Equals(this.GetLocation(enterNode.Graph.Id)));

            // TODO: Store outer edges instead of recreating them every time
            return OuterFlowEdge.CreateMethodCall(new OuterFlowEdgeId(-1), callNode, enterNode);
        }

        public async Task<IReadOnlyList<OuterFlowEdge>> GetCallEdgesToAsync(EnterFlowNode enterNode)
        {
            var results = new List<OuterFlowEdge>();

            var calledMethodLocation = this.GetLocation(enterNode.Graph.Id);
            var references = await SymbolFinder.FindCallersAsync(calledMethodLocation.Method, this.Solution);
            foreach (var reference in references)
            {
                Contract.Assert(reference.CalledSymbol.Equals(calledMethodLocation.Method));
                var callingMethod = reference.CallingSymbol as IMethodSymbol;
                if (callingMethod == null)
                {
                    continue;
                }

                var callingMethodLocation = new MethodLocation(callingMethod);
                if (!callingMethodLocation.CanBeExplored)
                {
                    continue;
                }

                var graphs = await this.LazyGenerateGraphsAsync(callingMethodLocation);
                foreach (var callNode in graphs.FlowGraph.Nodes.OfType<CallFlowNode>())
                {
                    if (((MethodLocation)callNode.Location).Equals(calledMethodLocation))
                    {
                        // TODO: Store outer edges instead of recreating them every time
                        var callEdge = OuterFlowEdge.CreateMethodCall(new OuterFlowEdgeId(-1), callNode, enterNode);
                        results.Add(callEdge);
                    }
                }
            }

            return results.ToArray();
        }

        public async Task<IReadOnlyList<OuterFlowEdge>> GetReturnEdgesToAsync(CallFlowNode callNode)
        {
            // TODO: Store outer edges instead of recreating them every time
            var graph = (await this.LazyGenerateGraphsAsync((MethodLocation)callNode.Location)).FlowGraph;
            return graph.Nodes
                .OfType<ReturnFlowNode>()
                .Select(returnNode => OuterFlowEdge.CreateReturn(new OuterFlowEdgeId(-1), returnNode, callNode))
                .ToArray();
        }

        private async Task<GeneratedGraphs> LazyGenerateGraphsAsync(MethodLocation location)
        {
            FlowGraphId graphId;
            GeneratedGraphs result;
            if (this.symbolsToGraphIdMap.TryGetValue((location).Method, out graphId))
            {
                result = this.generatedGraphs[graphId];
            }
            else
            {
                graphId = this.graphIdProvider.GenerateNewId();
                result = await Task.Run(() => this.GenerateGraphsImpl(location, graphId));

                this.generatedGraphs[graphId] = result;
                this.symbolsToGraphIdMap.Add(location.Method, graphId);
            }

            return result;
        }

        // TODO: Implement proper exception handling
        private async Task<GeneratedGraphs> GenerateGraphsImpl(MethodLocation location, FlowGraphId graphId)
        {
            var declarationLocation = location.Method.Locations.FirstOrDefault();
            Contract.Assert(declarationLocation != null);
            Contract.Assert(declarationLocation.IsInSource);

            var root = declarationLocation.SourceTree.GetRoot();
            var methodSyntax = root.FindNode(declarationLocation.SourceSpan) as BaseMethodDeclarationSyntax;
            Contract.Assert(methodSyntax != null);

            // TODO: Handle the continuation in a logic way
            var document = this.Solution.GetDocument(root.SyntaxTree);
            var semanticModel = document.GetSemanticModelAsync().Result;

            var builder = new CSharpGraphBuilder(
                this.ModelManager,
                document.Id,
                semanticModel,
                methodSyntax);

            var buildGraph = await builder.BuildAsync();

            var flowGraphTranslator = new FlowGraphTranslator(buildGraph, builder.DisplayGraph, graphId);
            var result = flowGraphTranslator.Translate();
            result.Location = location;

            return result;
        }
    }
}
