using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard
{

    public abstract class ExpressionVisitor<TResult>
    {
        public virtual TResult Visit(Expression expression)
        {
            if (expression != null)
            {
                return expression.Accept(this);
            }

            return default(TResult);
        }

        public virtual TResult DefaultVisit(Expression expression)
        {
            return default(TResult);
        }

        public virtual TResult VisitFunction(Function function)
        {
            return this.DefaultVisit(function);
        }

        public virtual TResult VisitInterpretation(Interpretation interpretation)
        {
            return this.DefaultVisit(interpretation);
        }

        public virtual TResult VisitVariable(Variable variable)
        {
            return this.DefaultVisit(variable);
        }
    }
}
