using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    public abstract class OperationVisitor<TResult>
    {
        public virtual TResult Visit(Operation operation)
        {
            if (operation != null)
            {
                return operation.Accept(this);
            }

            return default(TResult);
        }

        public virtual TResult DefaultVisit(Operation operation)
        {
            return default(TResult);
        }

        public virtual TResult VisitAssignment(Assignment assignment)
        {
            return this.DefaultVisit(assignment);
        }
    }
}
