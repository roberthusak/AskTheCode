using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.PathExploration.Heap;
using AskTheCode.SmtLibStandard;
using CodeContractsRevival.Runtime;

namespace AskTheCode.PathExploration
{
    public class ExecutionModel
    {
        public ExecutionModel(
            IHeapModel heapModel,
            ImmutableArray<FlowNode> pathNodes,
            ImmutableArray<ImmutableArray<Interpretation>> nodeInterpretations,
            ImmutableArray<ImmutableArray<HeapModelLocation>> heapLocations)
        {
            Contract.Requires(pathNodes != null);
            Contract.Requires(nodeInterpretations != null);
            Contract.Requires(heapLocations != null);
            Contract.Requires(heapModel != null);
            Contract.Requires(pathNodes.Length == nodeInterpretations.Length);
            Contract.Requires(nodeInterpretations.Length == heapLocations.Length);

            this.HeapModel = heapModel;
            this.PathNodes = pathNodes;
            this.NodeInterpretations = nodeInterpretations;
            this.HeapLocations = heapLocations;
        }

        public IHeapModel HeapModel { get; }

        public ImmutableArray<FlowNode> PathNodes { get; }

        public ImmutableArray<ImmutableArray<Interpretation>> NodeInterpretations { get; }

        public ImmutableArray<ImmutableArray<HeapModelLocation>> HeapLocations { get; }
    }
}
