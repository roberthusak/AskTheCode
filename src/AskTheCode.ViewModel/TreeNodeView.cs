using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AskTheCode.Core;
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
        }

        public ObservableCollection<TreeNodeView> Children { get; } = new ObservableCollection<TreeNodeView>();

        public string Name
        {
            get { return this.Node.Location.InspectedDeclaration.Identifier.ToString(); }
        }

        // TODO: Replace with some useful information
        public string Description
        {
            get { return this.Node.Location.InspectedDeclaration.ToFullString(); }
        }

        public ICommand ShowCommand { get; private set; }

        private InspectionNode Node { get; set; }

        public void Show()
        {
            var declaration = this.Node.Location.InspectedDeclaration;
            var sourceText = declaration.SyntaxTree.GetText();

            var highlights = new Dictionary<HighlightType, IEnumerable<TextSpan>>();
            highlights[HighlightType.Standard] = new[] { declaration.Identifier.Span };
            highlights[HighlightType.Dummy] = new[] { declaration.Body.Span };

            this.ideServices.HighlightText(sourceText, highlights);
        }

        public void Hide()
        {
            var declaration = this.Node.Location.InspectedDeclaration;
            var sourceText = declaration.SyntaxTree.GetText();

            var highlights = new Dictionary<HighlightType, IEnumerable<TextSpan>>();

            this.ideServices.HighlightText(sourceText, highlights);
        }
    }
}
