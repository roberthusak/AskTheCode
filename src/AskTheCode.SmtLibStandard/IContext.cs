using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.SmtLibStandard
{
    public interface IContext
    {
        ISolver CreateSolver(bool areDeclarationsGlobal, bool isUnsatisfiableCoreProduced);
    }
}
