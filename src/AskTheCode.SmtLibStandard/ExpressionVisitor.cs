using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard
{
    public abstract class ExpressionVisitor
    {
        public virtual void Visit(Expression expression)
        {
            if (expression != null)
            {
                expression.Accept(this);
            }
        }

        public virtual void DefaultVisit(Expression expression)
        {
        }

        public virtual void VisitFunction(Function function)
        {
            this.DefaultVisit(function);
        }

        public virtual void VisitInterpretation(Interpretation interpretation)
        {
            this.DefaultVisit(interpretation);
        }

        public virtual void VisitVariable(Variable variable)
        {
            this.DefaultVisit(variable);
        }
    }
}
