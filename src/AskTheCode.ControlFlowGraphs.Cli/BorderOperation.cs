using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;

namespace AskTheCode.ControlFlowGraphs.Cli
{
    internal class BorderOperation : SpecialOperation
    {
        public BorderOperation(SpecialOperationKind kind, IMethodSymbol method, IEnumerable<ITypeModel> arguments)
            : base(kind)
        {
            Contract.Requires(IsKindSupported(kind));
            Contract.Requires<ArgumentNullException>(
                (kind != SpecialOperationKind.MethodCall && kind != SpecialOperationKind.Assertion) || method != null,
                nameof(method));
            Contract.Requires<ArgumentNullException>(
                (kind != SpecialOperationKind.MethodCall && kind != SpecialOperationKind.Assertion) || arguments != null,
                nameof(arguments));

            this.Method = method;
            this.Arguments = arguments?.ToImmutableArray();
        }

        public IMethodSymbol Method { get; private set; }

        public IReadOnlyList<ITypeModel> Arguments { get; private set; }

        public static bool IsKindSupported(SpecialOperationKind kind)
        {
            switch (kind)
            {
                case SpecialOperationKind.Enter:
                case SpecialOperationKind.Return:
                case SpecialOperationKind.MethodCall:
                case SpecialOperationKind.ExceptionThrow:
                case SpecialOperationKind.Assertion:
                    return true;

                default:
                    return false;
            }
        }
    }
}
