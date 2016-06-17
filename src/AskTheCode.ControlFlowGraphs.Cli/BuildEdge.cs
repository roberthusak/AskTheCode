using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BuildEdge
    {
        public BuildEdge(BuildNode to, Expression valueCondition = null)
        {
            this.To = to;
            this.ValueCondition = valueCondition;
        }

        public BuildNode To { get; set; }

        public Expression ValueCondition { get; set; }

        public FlowEdge Edge { get; set; }
    }
}
