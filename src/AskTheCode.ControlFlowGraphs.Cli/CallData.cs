using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal enum CallDataKind
    {
        MethodCall,
        ExceptionThrow
    }

    internal class CallData
    {
        public CallData(CallDataKind kind, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            Contract.Requires<ArgumentNullException>(method != null, nameof(method));
            Contract.Requires<ArgumentNullException>(arguments != null, nameof(arguments));

            this.Kind = kind;
            this.Method = method;
            this.Arguments = arguments.ToImmutableArray();
        }

        public CallDataKind Kind { get; private set; }

        public IMethodSymbol Method { get; private set; }

        public IReadOnlyList<ITypeModel> Arguments { get; private set; }
    }
}
