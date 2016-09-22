using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using AskTheCode.ControlFlowGraphs.Cli;

namespace AskTheCode.ViewModel
{
    public class ToolView : NotifyPropertyChangedBase
    {
        private readonly IIdeServices ideServices;

        private FlowGraphView selectedFlowGraph;

        public ToolView(IIdeServices ideServices)
        {
            Contract.Requires<ArgumentNullException>(ideServices != null, nameof(ideServices));

            this.ideServices = ideServices;
            this.DisplayFlowGraphCommand = new Command(this.DisplayFlowGraph);

            // TODO: Initialize somehow lazily instead
            this.GraphProvider = new CSharpFlowGraphProvider(this.ideServices.Workspace.CurrentSolution);
        }

        public ObservableCollection<FlowGraphView> FlowGraphs { get; private set; } =
            new ObservableCollection<FlowGraphView>();

        public FlowGraphView SelectedFlowGraph
        {
            get { return this.selectedFlowGraph; }
            set { this.SetProperty(ref this.selectedFlowGraph, value); }
        }

        public Command DisplayFlowGraphCommand { get; private set; }

        // TODO: Properly handle the changing of the current solution
        internal CSharpFlowGraphProvider GraphProvider { get; private set; }

        public async void DisplayFlowGraph()
        {
            Document document;
            int position;
            if (!this.ideServices.TryGetCaretPosition(out document, out position))
            {
                return;
            }

            var root = await document.GetSyntaxRootAsync();
            if (root == null)
            {
                return;
            }

            var semanticModel = await document.GetSemanticModelAsync();
            var caretChildNode = root.FindNode(new TextSpan(position, 0));
            var methodDeclaration = caretChildNode.FirstAncestorOrSelf<MethodDeclarationSyntax>();
            if (semanticModel == null || methodDeclaration == null)
            {
                return;
            }

            var methodSymbol = semanticModel.GetDeclaredSymbol(methodDeclaration);
            if (methodSymbol == null)
            {
                return;
            }

            // TODO: Polish the usage of this thing
            var location = new MethodLocation(methodSymbol, true);
            var flowGraph = await this.GraphProvider.GetFlowGraphAsync(location);
            var displayGraph = this.GraphProvider.GetDisplayGraph(flowGraph.Id);

            var graphView = new FlowGraphView(document, location, flowGraph, displayGraph);
            this.FlowGraphs.Add(graphView);
            this.SelectedFlowGraph = graphView;
        }
    }
}
