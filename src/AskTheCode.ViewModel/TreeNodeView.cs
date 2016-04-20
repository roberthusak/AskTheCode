using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AskTheCode.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace AskTheCode.ViewModel
{
    public sealed class TreeNodeView : NotifyPropertyChangedBase
    {
        private readonly IIdeServices ideServices;

        internal TreeNodeView(IIdeServices ideServices, InspectionNode node)
        {
            Contract.Requires<ArgumentNullException>(ideServices != null, nameof(ideServices));
            Contract.Requires<ArgumentNullException>(node != null, nameof(node));

            this.ideServices = ideServices;
            this.Node = node;
            this.ShowCommand = new Command(this.Show);
            this.InspectCommand = new Command(this.Inspect);
        }

        public ObservableCollection<TreeNodeView> Children { get; } = new ObservableCollection<TreeNodeView>();

        public string Name
        {
            get { return this.Node.Location.DeclarationSymbol.Name; }
        }

        // TODO: Replace with some useful information
        public string Description
        {
            get { return this.Node.Location.Declaration.ToFullString(); }
        }

        public ICommand ShowCommand { get; private set; }

        public ICommand InspectCommand { get; private set; }

        private InspectionNode Node { get; set; }

        public async void Show()
        {
            var declaration = this.Node.Location.Declaration;

            // Get the ID of the document from the solution snapshot of the inspection context
            var oldDocument = this.Node.Context.Solution.GetDocument(declaration.SyntaxTree);
            var documentId = oldDocument?.Id;
            if (documentId == null)
            {
                return;
            }

            // This will change the current solution to one with the document opened if it was not opened already
            this.ideServices.OpenDocument(oldDocument);

            // Find the corresponding document in the current solution
            var document = this.ideServices.Workspace.CurrentSolution.GetDocument(documentId);
            if (document == null)
            {
                return;
            }

            // TODO: Somehow reflect the changes between those two (or rework this approach completely)

            var sourceText = await document.GetTextAsync();
            this.ideServices.SelectText(sourceText, new TextSpan(declaration.SpanStart, 0));

            var highlights = new Dictionary<HighlightType, IEnumerable<TextSpan>>();
            if (declaration is BaseMethodDeclarationSyntax)
            {
                var baseMethodDeclaration = (BaseMethodDeclarationSyntax)declaration;
                highlights[HighlightType.Dummy] = new[] { baseMethodDeclaration.Body.Span };

                if (baseMethodDeclaration is MethodDeclarationSyntax)
                {
                    var methodDeclaration = (MethodDeclarationSyntax)baseMethodDeclaration;
                    highlights[HighlightType.Standard] = new[] { methodDeclaration.Identifier.Span };
                }
            }
            else if (declaration is AccessorDeclarationSyntax)
            {
                var accessorDeclaration = (AccessorDeclarationSyntax)declaration;
                highlights[HighlightType.Dummy] = new[] { accessorDeclaration.Body.Span };
                highlights[HighlightType.Standard] = new[] { accessorDeclaration.Keyword.Span };
            }

            this.ideServices.HighlightText(sourceText, highlights);
        }

        public void Hide()
        {
            var declaration = this.Node.Location.Declaration;
            var sourceText = declaration.SyntaxTree.GetText();

            var highlights = new Dictionary<HighlightType, IEnumerable<TextSpan>>();

            this.ideServices.HighlightText(sourceText, highlights);
        }

        public async void Inspect()
        {
            await this.Node.Context.InspectNode(this.Node);
            this.Children.Clear();
            foreach (var childNode in this.Node.Children)
            {
                var childNodeView = new TreeNodeView(this.ideServices, childNode);
                this.Children.Add(childNodeView);
            }
        }
    }
}
