using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.ControlFlowGraphs;

namespace AskTheCode.PathExploration
{
    public interface IFinalNodeRecognizer
    {
        bool IsFinalNode(FlowNode node);
    }
}
