using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AskTheCode.Core
{
    public static class InspectionContextProvider
    {
        public static InspectionContext CreateContext(Workspace workspace)
        {
            Contract.Requires<ArgumentNullException>(workspace != null);

            return new InspectionContext(workspace);
        }
    }
}
