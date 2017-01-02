using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    // TODO: Make more sofisticated and customizable
    public static class AssertionMethodRecognizer
    {
        public static bool IsAssertionMethod(IMethodSymbol method)
        {
            return method.Name.EndsWith("Assert");
        }
    }
}
