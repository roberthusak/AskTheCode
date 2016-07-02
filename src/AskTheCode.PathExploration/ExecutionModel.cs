using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public class ExecutionModel
    {
        public IReadOnlyList<FlowNode> PathNodes
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IReadOnlyList<NodeInterpretations> Interpretations
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
