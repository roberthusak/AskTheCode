using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public interface IEntryPointRecognizer
    {
        IFlowGraphProvider FlowGraphProvider { get; set; }

        bool IsFinalNode(FlowNode node);
    }
}
