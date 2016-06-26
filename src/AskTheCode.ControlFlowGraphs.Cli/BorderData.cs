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
    // TODO: Consider adding also Enter kind (in order not to be handled separately in the translation)
    internal enum BorderDataKind
    {
        Return,
        MethodCall,
        ExceptionThrow
    }

    internal class BorderData
    {
        public BorderData(BorderDataKind kind, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            Contract.Requires<ArgumentNullException>(kind == BorderDataKind.Return || method != null, nameof(method));
            Contract.Requires<ArgumentNullException>(
                kind == BorderDataKind.Return || arguments != null, nameof(arguments));

            this.Kind = kind;
            this.Method = method;
            this.Arguments = arguments?.ToImmutableArray();
        }

        public BorderDataKind Kind { get; private set; }

        public IMethodSymbol Method { get; private set; }

        public IReadOnlyList<ITypeModel> Arguments { get; private set; }
    }
}
