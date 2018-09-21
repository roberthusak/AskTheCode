using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PerformanceEvaluation
{
    internal struct ResultRow
    {
        public const string Header = "heuristic,task,t1,n1,t,n";

        public string Heuristic;
        public string Program;
        public double? FirstCounterexampleTime;
        public int? FirstCounterexampleCalls;
        public double? TotalTime;
        public int? TotalCalls;

        public override string ToString()
        {
            return $"{Heuristic},{Program},{FirstCounterexampleTime},{FirstCounterexampleCalls},{TotalTime},{TotalCalls}";
        }
    }
}
