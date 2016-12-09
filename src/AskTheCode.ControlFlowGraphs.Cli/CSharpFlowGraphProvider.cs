using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

        public async Task<FlowGraph> GetFlowGraphAsync(ILocation location)
        {
            Contract.Requires<ArgumentException>(location is MethodLocation, nameof(location));

            var graphs = await this.LazyGenerateGraphsAsync((MethodLocation)location);

            return graphs.FlowGraph;
        }

        public async Task<DisplayGraph> GetDisplayGraphAsync(ILocation location)
        {
            Contract.Requires<ArgumentException>(location is MethodLocation, nameof(location));

            var graphs = await this.LazyGenerateGraphsAsync((MethodLocation)location);

            return graphs.DisplayGraph;
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
        private GeneratedGraphs GenerateGraphsImpl(MethodLocation location, FlowGraphId graphId)
        {
            var declarationLocation = location.Method.Locations.FirstOrDefault();
            Contract.Assert(declarationLocation != null);
            Contract.Assert(declarationLocation.IsInSource);

            var root = declarationLocation.SourceTree.GetRoot();
            var methodSyntax = root.FindNode(declarationLocation.SourceSpan) as MethodDeclarationSyntax;
            Contract.Assert(methodSyntax != null);

            // TODO: Handle the continuation in a logic way
            var document = this.Solution.GetDocument(root.SyntaxTree);
            var semanticModel = document.GetSemanticModelAsync().Result;

            var builder = new CSharpGraphBuilder(
                this.ModelManager,
                document.Id,
                semanticModel,
                methodSyntax);

            // TODO: Handle the continuation in a logic way (consider making BuildAsync() synchronous)
            var buildGraph = builder.BuildAsync().Result;

            var flowGraphTranslator = new FlowGraphTranslator(buildGraph, builder.DisplayGraph, graphId);
            var result = flowGraphTranslator.Translate();
            result.Location = location;

            return result;
        }
    }
}
