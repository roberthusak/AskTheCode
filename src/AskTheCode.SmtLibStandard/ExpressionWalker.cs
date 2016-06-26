using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard
{
    public abstract class ExpressionWalker : ExpressionVisitor
    {
        public override void DefaultVisit(Expression expression)
        {
            foreach (var child in expression.Children)
            {
                this.Visit(child);
            }
        }
    }
}
