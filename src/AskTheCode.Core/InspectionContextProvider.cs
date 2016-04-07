using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AskTheCode.Core
{
    public sealed class InspectionContextProvider
    {
        static InspectionContextProvider()
        {
            Default = new InspectionContextProvider();
        }

        private InspectionContextProvider()
        {
        }

        public static InspectionContextProvider Default { get; private set; }

        public InspectionContext CreateContext(Solution solution)
        {
            Contract.Requires<ArgumentNullException>(solution != null, nameof(solution));

            return new InspectionContext(solution);
        }
    }
}
