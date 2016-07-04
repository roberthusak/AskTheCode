using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard.Z3
{
    public class ContextFactory : IContextFactory
    {
        public IContext CreateContext()
        {
            return new Context();
        }
    }
}
