using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.ControlFlowGraphs.Operations
{
    /// <summary>
    /// Enables to process instances of <see cref="Operation"/> with a return value of type <see cref="TResult"/>.
    /// </summary>
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

        public virtual TResult VisitFieldRead(FieldRead fieldRead)
        {
            return this.DefaultVisit(fieldRead);
        }

        public virtual TResult VisitFieldWrite(FieldWrite fieldWrite)
        {
            return this.DefaultVisit(fieldWrite);
        }
    }
}
