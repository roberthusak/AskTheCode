using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            Contract.Requires<InvalidOperationException>(this.InspectionTreeRoot != null);

            var root = await document.GetSyntaxRootAsync();
            var semanticModel = await document.GetSemanticModelAsync();
            var sourceText = await root.SyntaxTree.GetTextAsync();

            int absolutePosition = sourceText.Lines[position.Line - 1].Start + position.Character;
            Contract.Assert(absolutePosition <= sourceText.Length); // TODO: Or '<'?

            var inspectedToken = root.FindToken(absolutePosition);  // TODO: Check it is valid
            var inspectedDeclaration = inspectedToken.Parent?.AncestorsAndSelf().OfType<MethodDeclarationSyntax>().FirstOrDefault();

            this.InspectionTreeRoot = new InspectionNode(
                this,
                null,
                new InspectionLocation(inspectedDeclaration),
                new InspectionConditions(expression));
        }

        public async Task InspectNode(InspectionNode node)
        {
            Contract.Requires<ArgumentException>(node.Context == this);

            node.State = InspectionNodeState.Exploring;

            // TODO: Here perform the inspection and create a children list with any available children

            node.Children = new List<InspectionNode>();

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
    }
}
