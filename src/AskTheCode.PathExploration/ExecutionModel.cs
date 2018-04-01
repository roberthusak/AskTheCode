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
            ImmutableArray<FlowNode> pathNodes,
            ImmutableArray<ImmutableArray<Interpretation>> nodeInterpretations,
            ImmutableArray<ImmutableArray<IReferenceModel>> referenceModels)
        {
            Contract.Requires(pathNodes != null);
            Contract.Requires(nodeInterpretations != null);
            Contract.Requires(referenceModels != null);
            Contract.Requires(pathNodes.Length == nodeInterpretations.Length);
            Contract.Requires(nodeInterpretations.Length == referenceModels.Length);

            this.PathNodes = pathNodes;
            this.NodeInterpretations = nodeInterpretations;
            this.ReferenceModels = referenceModels;
        }

        public ImmutableArray<FlowNode> PathNodes { get; }

        public ImmutableArray<ImmutableArray<Interpretation>> NodeInterpretations { get; }

        public ImmutableArray<ImmutableArray<IReferenceModel>> ReferenceModels { get; }
    }
}
