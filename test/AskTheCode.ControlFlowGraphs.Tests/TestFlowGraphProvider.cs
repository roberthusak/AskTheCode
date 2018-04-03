using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    /// <remarks>
    /// This class is immutable in order to be thread-safe.
    /// </remarks>
    public class TestFlowGraphProvider : IFlowGraphProvider
    {
        private readonly ReadOnlyDictionary<MethodInfo, FlowGraph> generatorToGraphMap;
        private readonly ImmutableArray<FlowGraph> graphs;
        private readonly ImmutableArray<ImmutableArray<CallFlowNode>> graphCallSites;
        private readonly ImmutableArray<TestRoutineLocation> locations;

        public TestFlowGraphProvider(Type generatorClass)
        {
            Contract.Requires(generatorClass != null);

            var generators = generatorClass
                .GetMethods(BindingFlags.Public | BindingFlags.Static)
                .Where(
                    methodInfo => methodInfo.ReturnType == typeof(FlowGraph)
                    && !methodInfo.ContainsGenericParameters
                    && methodInfo.GetParameters()
                        .Select(p => p.ParameterType)
                        .SequenceEqual(new[] { typeof(FlowGraphId) }));

            var generatorToGraphMap = new Dictionary<MethodInfo, FlowGraph>();
            var locations = new List<TestRoutineLocation>();
            var graphs = new List<FlowGraph>();

            // We have to generate all the graphs beforehand due to immutability
            int id = 0;
            foreach (var generator in generators)
            {
                var props = generator.GetCustomAttribute<GeneratedMethodPropertiesAttribute>()
                    ?? new GeneratedMethodPropertiesAttribute();

                var graph = (FlowGraph)generator.Invoke(null, new object[] { new FlowGraphId(id) });
                var location = new TestRoutineLocation(generator, props.IsConstructor);

                generatorToGraphMap.Add(generator, graph);
                graphs.Add(graph);
                locations.Add(location);

                id++;
            }

            // Gather a list of call nodes for each graph being called
            var graphCallSites = new List<CallFlowNode>[graphs.Count];
            foreach (var callNode in graphs.SelectMany(g => g.Nodes).OfType<CallFlowNode>())
            {
                if (callNode.Location.CanBeExplored)
                {
                    var calledGen = ((TestRoutineLocation)callNode.Location).Generator;
                    var calledId = generatorToGraphMap[calledGen].Id;

                    if (graphCallSites[calledId.Value] == null)
                    {
                        graphCallSites[calledId.Value] = new List<CallFlowNode>()
                        {
                            callNode
                        };
                    }
                    else
                    {
                        graphCallSites[calledId.Value].Add(callNode);
                    }
                }
            }

            this.generatorToGraphMap = new ReadOnlyDictionary<MethodInfo, FlowGraph>(generatorToGraphMap);
            this.graphs = graphs.ToImmutableArray();
            this.graphCallSites = graphCallSites
                .Select(c => c?.ToImmutableArray() ?? ImmutableArray<CallFlowNode>.Empty)
                .ToImmutableArray();
            this.locations = locations.ToImmutableArray();
            this.GeneratedMethodLocations = locations.ToImmutableArray();
        }

        public ImmutableArray<TestRoutineLocation> GeneratedMethodLocations { get; }

        public FlowGraph this[FlowGraphId graphId] => this.graphs[graphId.Value];

        public IRoutineLocation GetLocation(FlowGraphId graphId) => this.locations[graphId.Value];

        public Task<FlowGraph> GetFlowGraphAsync(IRoutineLocation location)
        {
            return Task.FromResult(this.generatorToGraphMap[((TestRoutineLocation)location).Generator]);
        }

        public OuterFlowEdge GetCallEdge(CallFlowNode callNode, EnterFlowNode enterNode)
        {
            // TODO: Store outer edges instead of recreating them every time
            return OuterFlowEdge.CreateMethodCall(new OuterFlowEdgeId(-1), callNode, enterNode);
        }

        public Task<IReadOnlyList<OuterFlowEdge>> GetCallEdgesToAsync(EnterFlowNode enterNode)
        {
            // TODO: Store outer edges instead of recreating them every time
            ImmutableArray<CallFlowNode> callSites = this.graphCallSites[enterNode.Graph.Id.Value];
            var result = callSites
                .Select(callNode => OuterFlowEdge.CreateMethodCall(new OuterFlowEdgeId(-1), callNode, enterNode))
                .ToArray();

            return Task.FromResult<IReadOnlyList<OuterFlowEdge>>(result);
        }

        public Task<IReadOnlyList<OuterFlowEdge>> GetReturnEdgesToAsync(CallFlowNode callNode)
        {
            // TODO: Store outer edges instead of recreating them every time
            var generator = ((TestRoutineLocation)callNode.Location).Generator;
            var graph = this.generatorToGraphMap[generator];
            var result =
                graph.Nodes
                .OfType<ReturnFlowNode>()
                .Select(returnNode => OuterFlowEdge.CreateReturn(new OuterFlowEdgeId(-1), returnNode, callNode))
                .ToArray();

            return Task.FromResult<IReadOnlyList<OuterFlowEdge>>(result);
        }
    }
}
