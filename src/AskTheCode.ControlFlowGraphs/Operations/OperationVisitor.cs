using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    public abstract class OperationVisitor
    {
        public virtual void Visit(Operation operation)
        {
            if (operation != null)
            {
                operation.Accept(this);
            }
        }

        public virtual void DefaultVisit(Operation operation)
        {
        }

        public virtual void VisitAssignment(Assignment assignment)
        {
            this.DefaultVisit(assignment);
        }
    }
}
