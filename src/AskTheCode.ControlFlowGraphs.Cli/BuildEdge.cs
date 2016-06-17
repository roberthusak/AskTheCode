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

        public BuildNode To { get; private set; }

        public Expression ValueCondition { get; private set; }

        // TODO: Set once semantic? Or remove completely and care about only in the second phase?
        public FlowEdge FlowEdge { get; set; }

        public BuildEdge WithTo(BuildNode to)
        {
            return new BuildEdge(to, this.ValueCondition);
        }

        public BuildEdge WithValueCondition(Expression valueCondition)
        {
            return new BuildEdge(this.To, valueCondition);
        }
    }
}
