using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard
{
    public class ExpressionRewriter : ExpressionVisitor<Expression>
    {
        public override Expression DefaultVisit(Expression expression)
        {
            return expression;
        }

        public override Expression VisitFunction(Function function)
        {
            int childrenCount = function.ChildrenCount;
            var updatedChildren = new Expression[childrenCount];

            bool areDifferent = false;
            for (int i = 0; i < function.ChildrenCount; i++)
            {
                Expression originalChild = function.GetChild(i);
                var updatedChild = this.Visit(originalChild);

                Contract.Assert(updatedChild != null);
                Contract.Assert(updatedChild.Sort == originalChild.Sort);

                if (updatedChild != originalChild)
                {
                    areDifferent = true;
                }

                updatedChildren[i] = updatedChild;
            }

            if (areDifferent)
            {
                return ExpressionFactory.Function(function.Kind, function.Sort, updatedChildren);
            }
            else
            {
                return function;
            }
        }
    }
}
