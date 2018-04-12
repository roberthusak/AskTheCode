using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.TypeSystem;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration.Heap
{
    // TODO: Add nested symbolic heap instance to delegate complicated operations to
    public class ProxySimplifyingSymbolicHeap : ISymbolicHeap
    {
        private readonly ISymbolicHeapContext context;
        private readonly Stack<ReferenceGraph> graphStack = new Stack<ReferenceGraph>();

        public ProxySimplifyingSymbolicHeap(ISymbolicHeapContext context)
        {
            this.context = context;

            this.graphStack.Push(ReferenceGraph.BasicGraph);
        }

        private ProxySimplifyingSymbolicHeap(ISymbolicHeapContext context, Stack<ReferenceGraph> graphStack)
        {
            this.context = context;

            foreach (var graph in graphStack.Reverse())
            {
                this.graphStack.Push(graph);
            }
        }

        public bool CanBeSatisfiable => this.CurrentGraph != ReferenceGraph.ConflictGraph;

        public ImmutableArray<BoolHandle> Assumptions => ImmutableArray<BoolHandle>.Empty;

        private ReferenceGraph CurrentGraph => this.graphStack.Peek();

        public ProxySimplifyingSymbolicHeap Clone(ISymbolicHeapContext context)
        {
            return new ProxySimplifyingSymbolicHeap(context, this.graphStack);
        }

        ISymbolicHeap ISymbolicHeap.Clone(ISymbolicHeapContext context) => this.Clone(context);

        public void AllocateNew(VersionedVariable result)
        {
            if (!this.CanBeSatisfiable)
            {
                this.graphStack.Push(ReferenceGraph.ConflictGraph);
                return;
            }

            var newGraph = this.CurrentGraph.MakeInequal(result, VersionedVariable.Null);
            this.graphStack.Push(newGraph);
        }

        public void AssertEquality(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            if (!this.CanBeSatisfiable)
            {
                this.graphStack.Push(ReferenceGraph.ConflictGraph);
                return;
            }

            ReferenceGraph newGraph;
            if (areEqual)
            {
                newGraph = this.CurrentGraph.MakeEqual(left, right, this.OnNodeMerge);
            }
            else
            {
                newGraph = this.CurrentGraph.MakeInequal(left, right);
            }

            this.graphStack.Push(newGraph);
        }

        public Expression GetEqualityExpression(bool areEqual, VersionedVariable left, VersionedVariable right)
        {
            // It makes no change to the graph
            this.graphStack.Push(this.CurrentGraph);

            if (!this.CanBeSatisfiable)
            {
                // Doesn't matter, the path is unreachable anyway
                return ExpressionFactory.False;
            }

            bool? result = this.CurrentGraph.GetEquality(left, right);
            if (result != null)
            {
                return ExpressionFactory.BoolInterpretation(areEqual ? result.Value : !result.Value);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void ReadField(VersionedVariable result, VersionedVariable reference, IFieldDefinition field)
        {
            if (!this.CanBeSatisfiable)
            {
                this.graphStack.Push(ReferenceGraph.ConflictGraph);
                return;
            }

            var newGraph = this.CurrentGraph.PerformRead(result, reference, field, this.OnNodeMerge);
            this.graphStack.Push(newGraph);
        }

        public void WriteField(VersionedVariable reference, IFieldDefinition field, Expression value)
        {
            if (!this.CanBeSatisfiable)
            {
                this.graphStack.Push(ReferenceGraph.ConflictGraph);
                return;
            }

            // TODO: Support arbitrary expression
            var valueVar = this.context.GetVersioned((FlowVariable)value);

            var newGraph = this.CurrentGraph.PerformWrite(reference, field, valueVar, this.OnNodeMerge);
            this.graphStack.Push(newGraph);
        }

        public void Retract(int operationCount = 1)
        {
            for (int i = 0; i < operationCount; i++)
            {
                this.graphStack.Pop();
            }
        }

        public IHeapModelRecorder GetModelRecorder(IModel smtModel)
        {
            throw new NotImplementedException();
        }

        private void OnNodeMerge(ReferenceNode a, ReferenceNode b, ReferenceNode merged)
        {
            Contract.Requires(a != b && b != merged);

            var aRepresentative = a.Variables[0];
            if (!aRepresentative.Variable.IsReference)
            {
                var bRepresentative = b.Variables[0];
                Contract.Assert(aRepresentative != bRepresentative);
                Contract.Assert(aRepresentative.Variable.Sort == bRepresentative.Variable.Sort);

                var aVar = this.context.GetNamedVariable(aRepresentative);
                var bVar = this.context.GetNamedVariable(bRepresentative);
                this.context.AddAssertion((BoolHandle)ExpressionFactory.Equal(aVar, bVar));
            }
        }

        private struct ReferenceEdge
        {
            public ReferenceEdge(
                int targetNodeId,
                ReferenceGraph.ReferenceOperation lastOperation)
            {
                this.TargetNodeId = targetNodeId;
                this.LastOperation = lastOperation;
            }

            public int TargetNodeId { get; }

            public ReferenceGraph.ReferenceOperation LastOperation { get; }

            public override string ToString()
            {
                char operationChar = (this.LastOperation == ReferenceGraph.ReferenceOperation.Read) ? 'R' : 'W';
                return $"--({operationChar})-->[{this.TargetNodeId}]";
            }
        }

        private class ReferenceNode
        {
            public ReferenceNode(int id, ImmutableList<VersionedVariable> variables)
            {
                this.Id = id;
                this.Variables = variables;
            }

            public int Id { get; }

            public ImmutableList<VersionedVariable> Variables { get; }

            public override string ToString() => $"[{this.Id}] ({string.Join(", ", this.Variables)})";
        }

        private class ReferenceGraph
        {
            /// <summary>
            /// A special instance to represent graphs with conflicts.
            /// </summary>
            public static readonly ReferenceGraph ConflictGraph = new ReferenceGraph(null, null, null, null, -1);

            public static readonly ReferenceGraph BasicGraph = ConstructBasicGraph();

            private const int NullNodeId = 0;
            private const int InvalidNodeId = -1;

            private readonly ImmutableSortedDictionary<int, ReferenceNode> nodes;
            private readonly ImmutableSortedDictionary<int, ImmutableDictionary<IFieldDefinition, ReferenceEdge>> nodeEdges;
            private readonly ImmutableSortedDictionary<int, ImmutableSortedSet<int>> nodeInequalities;
            private readonly ImmutableDictionary<VersionedVariable, int> variableToNodeIdMap;
            private readonly int nextNodeId;

            private ReferenceGraph(
                ImmutableSortedDictionary<int, ReferenceNode> nodes,
                ImmutableSortedDictionary<int, ImmutableDictionary<IFieldDefinition, ReferenceEdge>> nodeEdges,
                ImmutableSortedDictionary<int, ImmutableSortedSet<int>> nodeInequalities,
                ImmutableDictionary<VersionedVariable, int> variableToNodeIdMap,
                int nextNodeId)
            {
                this.nodes = nodes;
                this.nodeEdges = nodeEdges;
                this.nodeInequalities = nodeInequalities;
                this.variableToNodeIdMap = variableToNodeIdMap;
                this.nextNodeId = nextNodeId;
            }

            public delegate void MergeListener(ReferenceNode a, ReferenceNode b, ReferenceNode merged);

            public enum ReferenceOperation
            {
                Read,
                Write
            }

            public ReferenceGraph MakeEqual(
                VersionedVariable left,
                VersionedVariable right,
                MergeListener mergeListener)
            {
                // TODO: Consider optimizing by adding an unknown variable to a known one's group
                //       or creating a group of two unknown variables at once
                (var graphWithLeft, var leftNode) = this.GetOrAddNode(left);
                (var graph, var rightNode) = graphWithLeft.GetOrAddNode(right);

                if (leftNode == rightNode)
                {
                    return graph;
                }
                else
                {
                    return graph.MergeNodes(leftNode.Id, rightNode.Id, mergeListener);
                }
            }

            public ReferenceGraph MakeInequal(VersionedVariable left, VersionedVariable right)
            {
                (var graphWithLeft, var leftNode) = this.GetOrAddNode(left);
                (var graph, var rightNode) = graphWithLeft.GetOrAddNode(right);

                if (leftNode == rightNode)
                {
                    return ConflictGraph;
                }

                var ineqBuilder = graph.nodeInequalities.ToBuilder();

                var leftInequals = ineqBuilder[leftNode.Id];
                if (!leftInequals.Contains(rightNode.Id))
                {
                    ineqBuilder[leftNode.Id] = leftInequals.Add(rightNode.Id);
                }

                var rightInequals = ineqBuilder[rightNode.Id];
                if (!rightInequals.Contains(leftNode.Id))
                {
                    ineqBuilder[rightNode.Id] = rightInequals.Add(leftNode.Id);
                }

                var inequalities = ineqBuilder.ToImmutable();
                if (inequalities == this.nodeInequalities)
                {
                    return graph;
                }
                else
                {
                    return new ReferenceGraph(
                        graph.nodes,
                        graph.nodeEdges,
                        inequalities,
                        graph.variableToNodeIdMap,
                        graph.nextNodeId);
                }
            }

            public bool? GetEquality(VersionedVariable left, VersionedVariable right)
            {
                if (this.variableToNodeIdMap.TryGetValue(left, out int leftId)
                    && this.variableToNodeIdMap.TryGetValue(right, out int rightId))
                {
                    if (leftId == rightId)
                    {
                        return true;
                    }
                    else if (this.nodeInequalities[leftId].Contains(rightId))
                    {
                        return false;
                    }
                }

                return null;
            }

            public ReferenceGraph PerformRead(
                VersionedVariable result,
                VersionedVariable reference,
                IFieldDefinition field,
                MergeListener mergeListener)
            {
                (var graphWithResult, var resultNode) = this.GetOrAddNode(result);
                (var graph, var refNode) = graphWithResult.GetOrAddNode(reference);

                if (graph.nodeEdges[refNode.Id].TryGetValue(field, out var edge))
                {
                    if (edge.LastOperation == ReferenceOperation.Read)
                    {
                        // Add to the target variables
                        if (edge.TargetNodeId != resultNode.Id)
                        {
                            return graph.MergeNodes(edge.TargetNodeId, resultNode.Id, mergeListener);
                        }
                        else
                        {
                            return graph;
                        }
                    }
                    else
                    {
                        Contract.Assert(edge.LastOperation == ReferenceOperation.Write);

                        // Discard the information about the last write to this field and create a new read
                        var newEdge = new ReferenceEdge(resultNode.Id, ReferenceOperation.Read);
                        var newNodeEdges = graph.nodeEdges.SetItem(
                            refNode.Id,
                            graph.nodeEdges[refNode.Id].SetItem(field, newEdge));

                        return new ReferenceGraph(
                            graph.nodes,
                            newNodeEdges,
                            graph.nodeInequalities,
                            graph.variableToNodeIdMap,
                            graph.nextNodeId);
                    }
                }
                else
                {
                    // Add a new connection to the variable being read from the field
                    var newEdge = new ReferenceEdge(resultNode.Id, ReferenceOperation.Read);
                    var newNodeEdges = graph.nodeEdges.SetItem(
                        refNode.Id,
                        graph.nodeEdges[refNode.Id].Add(field, newEdge));

                    return new ReferenceGraph(
                        graph.nodes,
                        newNodeEdges,
                        graph.nodeInequalities,
                        graph.variableToNodeIdMap,
                        graph.nextNodeId);
                }
            }

            public ReferenceGraph PerformWrite(
                VersionedVariable reference,
                IFieldDefinition field,
                VersionedVariable value,
                MergeListener mergeListener)
            {
                int nodeIdNotToClearEdges = InvalidNodeId;
                var graph = this;

                if (this.variableToNodeIdMap.TryGetValue(reference, out int refNodeId)
                    && this.nodeEdges[refNodeId].TryGetValue(field, out var edge))
                {
                    // This write might be overriden by a subsequent one (if the last edge operation is write),
                    // but the result value is clear anyway, so we don't need to erase this particular link.
                    nodeIdNotToClearEdges = refNodeId;

                    if (edge.LastOperation == ReferenceOperation.Read)
                    {
                        // TODO: Consider optimizing by adding an unknown variable to a known one's group
                        ReferenceNode valueNode;
                        (graph, valueNode) = this.GetOrAddNode(value);

                        if (edge.TargetNodeId != valueNode.Id)
                        {
                            graph = graph.MergeNodes(edge.TargetNodeId, valueNode.Id, mergeListener);

                            if (graph == ConflictGraph)
                            {
                                return ConflictGraph;
                            }
                        }
                    }
                }

                // Discard the links of the related field from the other nodes,
                // because the write might have changed their structure.
                var nodeEdgesBuilder = graph.nodeEdges.ToBuilder();
                foreach (int id in graph.nodes.Keys)
                {
                    if (id == nodeIdNotToClearEdges)
                    {
                        continue;
                    }

                    nodeEdgesBuilder[id] = graph.nodeEdges[id].Remove(field);
                }

                var newNodeEdges = nodeEdgesBuilder.ToImmutable();
                if (newNodeEdges == graph.nodeEdges)
                {
                    return graph;
                }
                else
                {
                    return new ReferenceGraph(
                        graph.nodes,
                        newNodeEdges,
                        graph.nodeInequalities,
                        graph.variableToNodeIdMap,
                        graph.nextNodeId);
                }
            }

            private static ReferenceGraph ConstructBasicGraph()
            {
                int nextId = 0;
                var nullNode = new ReferenceNode(nextId++, ImmutableList.Create(VersionedVariable.Null));

                var nodes = ImmutableSortedDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, ReferenceNode>(nullNode.Id, nullNode)
                });
                var nodeEdges = ImmutableSortedDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, ImmutableDictionary<IFieldDefinition, ReferenceEdge>>(
                        nullNode.Id,
                        ImmutableDictionary<IFieldDefinition, ReferenceEdge>.Empty)
                });
                var inequalities = ImmutableSortedDictionary.CreateRange(new[]
                {
                    new KeyValuePair<int, ImmutableSortedSet<int>>(nullNode.Id, ImmutableSortedSet<int>.Empty),
                });
                var varNodeMap = ImmutableDictionary.CreateRange(new[]
                {
                    new KeyValuePair<VersionedVariable, int>(VersionedVariable.Null, nullNode.Id)
                });

                return new ReferenceGraph(nodes, nodeEdges, inequalities, varNodeMap, nextId);
            }

            private (ReferenceGraph newGraph, ReferenceNode node) GetOrAddNode(VersionedVariable variable)
            {
                if (this.variableToNodeIdMap.TryGetValue(variable, out int nodeId))
                {
                    return (this, this.nodes[nodeId]);
                }
                else
                {
                    var node = new ReferenceNode(this.nextNodeId, ImmutableList.Create(variable));
                    var newGraph = new ReferenceGraph(
                        this.nodes.Add(node.Id, node),
                        this.nodeEdges.Add(node.Id, ImmutableDictionary<IFieldDefinition, ReferenceEdge>.Empty),
                        this.nodeInequalities.Add(node.Id, ImmutableSortedSet<int>.Empty),
                        this.variableToNodeIdMap.Add(variable, node.Id),
                        this.nextNodeId + 1);
                    return (newGraph, node);
                }
            }

            private ReferenceGraph MergeNodes(int leftNodeId, int rightNodeId, MergeListener mergeListener)
            {
                Contract.Assert(leftNodeId != rightNodeId);

                var nodesBuilder = this.nodes.ToBuilder();
                var edgesBuilder = this.nodeEdges.ToBuilder();
                var inequalitiesBuilder = this.nodeInequalities.ToBuilder();
                var varNodeMapBuilder = this.variableToNodeIdMap.ToBuilder();

                // Multiple merges may be caused, process them in a FIFO fashion
                var mergeQueue = new Queue<(int toMergeId, int toRemoveId)>();
                mergeQueue.Enqueue((leftNodeId, rightNodeId));

                // Used to handle recursive merges, where the IDs were removed from the collections
                var removedIdMap = new SortedDictionary<int, int>();

                while (mergeQueue.Count > 0)
                {
                    (int toMergeId, int toRemoveId) = mergeQueue.Dequeue();

                    if (removedIdMap.TryGetValue(toMergeId, out int newMergeId))
                    {
                        toMergeId = newMergeId;
                    }

                    if (removedIdMap.TryGetValue(toRemoveId, out var newRemoveId))
                    {
                        toRemoveId = newRemoveId;
                    }

                    if (toMergeId == toRemoveId)
                    {
                        // They have been already merged
                        continue;
                    }
                    else if (this.nodeInequalities[toMergeId].Contains(toRemoveId))
                    {
                        // They can't be merged due to a conflict
                        return ConflictGraph;
                    }

                    // Merge nodes into the first one
                    var toMerge = nodesBuilder[toMergeId];
                    var toRemove = nodesBuilder[toRemoveId];
                    var merged = new ReferenceNode(
                        toMergeId,
                        toMerge.Variables.AddRange(toRemove.Variables));

                    mergeListener(toMerge, toRemove, merged);

                    nodesBuilder[toMergeId] = merged;
                    nodesBuilder.Remove(toRemoveId);

                    // Merge inequalities
                    if (inequalitiesBuilder[toRemoveId].Count > 0)
                    {
                        var mergedInequalsBuilder = inequalitiesBuilder[toMergeId].ToBuilder();
                        foreach (int inequalId in inequalitiesBuilder[toRemoveId])
                        {
                            mergedInequalsBuilder.Add(inequalId);
                            inequalitiesBuilder[inequalId] = inequalitiesBuilder[inequalId]
                                .Remove(toRemoveId)
                                .Add(toMergeId);
                        }

                        inequalitiesBuilder[toMergeId] = mergedInequalsBuilder.ToImmutable();
                    }

                    inequalitiesBuilder.Remove(toRemoveId);

                    // Merge fields and possible cause another merges
                    var toMergeEdgesBuilder = edgesBuilder[toMergeId].ToBuilder();
                    foreach (var toRemoveKvp in edgesBuilder[toRemoveId])
                    {
                        if (toMergeEdgesBuilder.TryGetValue(toRemoveKvp.Key, out var toMergeEdge))
                        {
                            if (toMergeEdge.TargetNodeId != toRemoveKvp.Value.TargetNodeId)
                            {
                                // Note that we set the node ID referenced from the currently merged node so that
                                // we don't have to edit its edges (with the read/write flag exception below)
                                mergeQueue.Enqueue((toMergeEdge.TargetNodeId, toRemoveKvp.Value.TargetNodeId));
                            }

                            // TODO: Verify if this is the right way how to merge the read and write flags
                            if (toMergeEdge.LastOperation == ReferenceOperation.Read
                                && toRemoveKvp.Value.LastOperation == ReferenceOperation.Write)
                            {
                                ReferenceEdge newEdge = new ReferenceEdge(
                                    toMergeEdge.TargetNodeId,
                                    ReferenceOperation.Write);
                                toMergeEdgesBuilder[toRemoveKvp.Key] = newEdge;
                            }
                        }
                        else
                        {
                            toMergeEdgesBuilder.Add(toRemoveKvp);
                        }
                    }

                    edgesBuilder[toMergeId] = toMergeEdgesBuilder.ToImmutable();
                    edgesBuilder.Remove(toRemoveId);

                    // Reflect the merge in the maps of removed IDs
                    // (it should be small, so the loop shouldn't take long)
                    var idsPointingToRemoveId = removedIdMap
                        .Where(kvp => kvp.Value == toRemoveId)
                        .Select(kvp => kvp.Key).ToArray();
                    removedIdMap[toRemoveId] = toMergeId;
                    foreach (int id in idsPointingToRemoveId)
                    {
                        removedIdMap[id] = toMergeId;
                    }
                }

                // Correct the edges and variable mappings to reflect the merging
                foreach (int nodeId in nodesBuilder.Keys)
                {
                    var nodeEdgesBuilder = edgesBuilder[nodeId].ToBuilder();
                    foreach (var kvp in edgesBuilder[nodeId])
                    {
                        if (removedIdMap.TryGetValue(kvp.Value.TargetNodeId, out int newId))
                        {
                            var newEdge = new ReferenceEdge(newId, kvp.Value.LastOperation);
                            nodeEdgesBuilder[kvp.Key] = newEdge;
                        }
                    }

                    edgesBuilder[nodeId] = nodeEdgesBuilder.ToImmutable();
                }

                foreach (var kvp in this.variableToNodeIdMap)
                {
                    if (removedIdMap.TryGetValue(kvp.Value, out int newId))
                    {
                        varNodeMapBuilder[kvp.Key] = newId;
                    }
                }

                return new ReferenceGraph(
                    nodesBuilder.ToImmutable(),
                    edgesBuilder.ToImmutable(),
                    inequalitiesBuilder.ToImmutable(),
                    varNodeMapBuilder.ToImmutable(),
                    this.nextNodeId);
            }
        }
    }

    public class ProxySimplifyingSymbolicHeapFactory : ISymbolicHeapFactory
    {
        public ISymbolicHeap Create(ISymbolicHeapContext context)
        {
            return new ProxySimplifyingSymbolicHeap(context);
        }
    }
}
