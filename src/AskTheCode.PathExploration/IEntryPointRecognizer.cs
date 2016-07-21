using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public interface IEntryPointRecognizer
    {
        bool IsFinalNode(FlowNode node);
    }
}
