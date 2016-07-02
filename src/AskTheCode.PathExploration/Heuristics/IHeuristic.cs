using System;
using System.Collections.Generic;
using System.Text;

namespace AskTheCode.PathExploration.Heuristics
{
    public interface IHeuristic
    {
        void Initialize(Explorer explorer);
    }
}
