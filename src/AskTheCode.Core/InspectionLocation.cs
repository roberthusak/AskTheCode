using System;
using System.Diagnostics.Contracts;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.Core
{
    public sealed class InspectionLocation
    {
        internal InspectionLocation(SyntaxNode declaration, IMethodSymbol declarationSymbol)
        {
            Contract.Requires<ArgumentNullException>(declaration != null, nameof(declaration));
            Contract.Requires<ArgumentNullException>(declarationSymbol != null, nameof(declarationSymbol));

            this.Declaration = declaration;
            this.DeclarationSymbol = declarationSymbol;
        }

        public SyntaxNode Declaration { get; private set; }

        public IMethodSymbol DeclarationSymbol { get; private set; }
    }
}