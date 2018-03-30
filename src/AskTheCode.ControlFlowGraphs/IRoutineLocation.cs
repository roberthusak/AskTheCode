using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.ControlFlowGraphs
{
    public interface IRoutineLocation
    {
        bool CanBeExplored { get; }

        bool IsConstructor { get; }
    }
}
