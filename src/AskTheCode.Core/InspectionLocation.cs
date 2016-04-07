using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AskTheCode.Core
{
    public class InspectionLocation
    {
        internal InspectionLocation(MethodDeclarationSyntax inspectedDeclaration)
        {
            this.InspectedDeclaration = inspectedDeclaration;
        }

        public MethodDeclarationSyntax InspectedDeclaration { get; private set; }
    }
}