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
    internal enum BorderDataKind
    {
        Enter,
        Return,
        MethodCall,
        ExceptionThrow,
        Assertion,
    }

    internal class BorderData
    {
        public BorderData(BorderDataKind kind, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
        {
            Contract.Requires<ArgumentNullException>(
                (kind != BorderDataKind.MethodCall && kind != BorderDataKind.Assertion) || method != null,
                nameof(method));
            Contract.Requires<ArgumentNullException>(
                (kind != BorderDataKind.MethodCall && kind != BorderDataKind.Assertion) || arguments != null,
                nameof(arguments));

            this.Kind = kind;
            this.Method = method;
            this.Arguments = arguments?.ToImmutableArray();
        }

        public BorderDataKind Kind { get; private set; }

        public IMethodSymbol Method { get; private set; }

        public IReadOnlyList<ITypeModel> Arguments { get; private set; }
    }
}
