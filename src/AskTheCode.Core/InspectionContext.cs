using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Shared.Extensions;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.Core
{
    public sealed class InspectionContext
    {
        internal InspectionContext(Solution solution)
        {
            Contract.Requires<ArgumentNullException>(solution != null, nameof(solution));

            this.Solution = solution;
        }

        public Solution Solution { get; private set; }

        public InspectionNode InspectionTreeRoot { get; private set; }

        public async Task StartInspecting(Document document, LinePosition position, string expression)
        {
            Contract.Requires<ArgumentNullException>(document != null, nameof(document));
            Contract.Requires<ArgumentException>(document.Project != null);
            Contract.Requires<ArgumentException>(document.Project.Solution == this.Solution);
            Contract.Requires<ArgumentException>(!string.IsNullOrEmpty(expression));
            Contract.Requires<InvalidOperationException>(this.InspectionTreeRoot == null);

            var root = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var sourceText = await root.SyntaxTree.GetTextAsync();

            int absolutePosition = sourceText.Lines[position.Line - 1].Start + position.Character;
            Contract.Assert(absolutePosition <= sourceText.Length); // TODO: Or '<'?

            var inspectedToken = root.FindToken(absolutePosition);  // TODO: Check it is valid
            var syntaxNode = inspectedToken.Parent;

            // TODO: Create a sophisticated validation mechanism to give appropriate information to the end user
            Contract.Assert(syntaxNode != null);

            // TODO: Implement real conditions handling instead of this
            var inspectionConditions = new InspectionConditions(expression);

            this.InspectionTreeRoot = this.CreateNode(semanticModel, syntaxNode, null, inspectionConditions);
        }

        public async Task InspectNode(InspectionNode node)
        {
            Contract.Requires<ArgumentException>(node.Context == this);

            node.State = InspectionNodeState.Exploring;

            // Here perform the inspection and create a children list with any available children
            var referencedSymbols = await SymbolFinder.FindReferencesAsync(
                node.Location.DeclarationSymbol,
                this.Solution);
            var children = new List<InspectionNode>();
            foreach (var referenceLocation in referencedSymbols.SelectMany(rs => rs.Locations))
            {
                Contract.Assert(referenceLocation.Location.IsInSource);

                var childDocument = referenceLocation.Document;
                var childTree = referenceLocation.Location.SourceTree;
                var childRoot = await childTree.GetRootAsync();
                var childSemanticModel = await childDocument.GetSemanticModelAsync();
                var childSyntaxNode = childRoot.FindNode(referenceLocation.Location.SourceSpan);

                // TODO: Infer the conditions appropriately
                var child = this.CreateNode(childSemanticModel, childSyntaxNode, node, node.Conditions);
                children.Add(child);
            }

            node.Children = children;

            // If the exploring finished, propagate the state to the tree root
            if (node.Children.Count() == 0)
            {
                node.State = InspectionNodeState.Explored;
                for (var ancestor = node.Parent; ancestor != null; ancestor = ancestor.Parent)
                {
                    if (ancestor.Children.All(child => child.State == InspectionNodeState.Explored))
                    {
                        ancestor.State = InspectionNodeState.Explored;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        private InspectionNode CreateNode(
            SemanticModel semanticModel,
            SyntaxNode syntaxNode,
            InspectionNode parent,
            InspectionConditions inspectionConditions)
        {
            // FIXME (for static methods and static field initializers?)
            var containingDeclarationsCollection =
                from node in syntaxNode.AncestorsAndSelf()
                where node is AccessorDeclarationSyntax || node is MemberDeclarationSyntax
                //where node is BaseMethodDeclarationSyntax || node is AccessorDeclarationSyntax
                //    || node is PropertyDeclarationSyntax || node is BaseFieldDeclarationSyntax
                select node;
            var inspectedDeclaration = containingDeclarationsCollection.FirstOrDefault();
            var inspectedSymbol = semanticModel.GetDeclaredSymbol(inspectedDeclaration) as IMethodSymbol;

            // TODO: Create a sophisticated validation mechanism to give appropriate information to the end user
            Contract.Assert(inspectedDeclaration != null);
            Contract.Assert(inspectedSymbol != null);

            return new InspectionNode(
                this,
                parent,
                new InspectionLocation(inspectedDeclaration, inspectedSymbol),
                inspectionConditions);
        }
    }
}
