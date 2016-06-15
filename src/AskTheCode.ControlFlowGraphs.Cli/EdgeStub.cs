using System;
using System.Collections.Generic;
using System.Text;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class EdgeStub
    {
        public EdgeStub(NodeStub to, Expression valueCondition = null)
        {
            this.To = to;
            this.ValueCondition = valueCondition;
        }

        public NodeStub To { get; set; }

        public Expression ValueCondition { get; set; }

        public FlowEdge Edge { get; set; }
    }
}
