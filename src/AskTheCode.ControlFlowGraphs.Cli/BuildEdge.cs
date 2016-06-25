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

        // TODO: Replace by IValueModel when the BooleanModelFactory.True and False are made public
        public Expression ValueCondition { get; private set; }

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
