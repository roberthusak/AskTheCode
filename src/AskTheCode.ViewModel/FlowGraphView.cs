using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using CodeContractsRevival.Runtime;
using Microsoft.CodeAnalysis;
using Microsoft.Msagl.Drawing;

namespace AskTheCode.ViewModel
{
    public class FlowGraphView : NotifyPropertyChangedBase, IGraphViewerConsumer
    {
        private IViewer graphViewer;

        internal FlowGraphView(Document document, MethodLocation location, FlowGraph flowGraph, DisplayGraph displayGraph)
        {
            this.Document = document;
            this.Location = location;
            this.FlowGraph = flowGraph;
            this.DisplayGraph = displayGraph;
        }

        // TODO: Acquire a proper one
        public string Header
        {
            get { return this.Location.ToString(); }
        }

        public Document Document { get; private set; }

        public MethodLocation Location { get; private set; }

        public FlowGraph FlowGraph { get; private set; }

        public DisplayGraph DisplayGraph { get; private set; }

        public IViewer GraphViewer
        {
            get { return this.graphViewer; }
            set { this.SetProperty(ref this.graphViewer, value); }
        }

        protected override void OnPropertyChanged<T>(string propertyName, T previousValue)
        {
            if (propertyName == nameof(this.GraphViewer) && this.GraphViewer != null)
            {
                this.Redraw();
            }
        }

        private async void Redraw()
        {
            Contract.Requires(this.GraphViewer != null);

            var text = await this.Document.GetTextAsync();

            var msaglGraph = new Graph();

            foreach (var displayNode in this.DisplayGraph.Nodes)
            {
                var msaglNode = msaglGraph.AddNode(displayNode.Id.Value.ToString());
                msaglNode.Label = new Label(text.ToString(displayNode.Span));
            }

            foreach (var displayNode in this.DisplayGraph.Nodes)
            {
                foreach (var displayEdge in displayNode.OutgoingEdges)
                {
                    msaglGraph.AddEdge(
                        displayNode.Id.Value.ToString(),
                        displayEdge.Label,
                        displayEdge.To.Id.Value.ToString());
                }
            }

            this.GraphViewer.Graph = msaglGraph;
        }
    }
}
