using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public interface ILocation
    {
        bool CanBeExplored { get; }
    }
}
