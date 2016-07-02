using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Text;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.PathExploration
{
    public class ExecutionModel
    {
        public ExecutionModel(
            ImmutableArray<FlowNode> pathNodes,
            ImmutableArray<ImmutableArray<Interpretation>> nodeInterpretations)
        {
            Contract.Requires(pathNodes != null);
            Contract.Requires(nodeInterpretations != null);

            this.PathNodes = pathNodes;
            this.NodeInterpretations = nodeInterpretations;
        }

        public ImmutableArray<FlowNode> PathNodes { get; private set; }

        public ImmutableArray<ImmutableArray<Interpretation>> NodeInterpretations { get; private set; }
    }
}
