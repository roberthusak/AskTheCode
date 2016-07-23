using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.SmtLibStandard;

namespace AskTheCode.PathExploration
{
    // TODO: Consider making the solution of this task a little cleaner
    internal class FlowVariableAssignmentWrapper : Variable
    {
        public FlowVariableAssignmentWrapper(FlowVariable variable)
            : base(variable.Sort)
        {
            Contract.Requires(variable != null);

            this.Variable = variable;
        }

        // TODO: Consider altering the name so that it is apparent it is another version
        public override string DisplayName
        {
            get { return this.Variable.DisplayName; }
        }

        public FlowVariable Variable { get; private set; }
    }
}
