using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.SmtLibStandard;
using Microsoft.Msagl.Drawing;

namespace ControlFlowGraphViewer
{
    internal class CSharpBuildToMsaglGraphConverter
    {
        public Graph Convert(CSharpFlowGraphBuilder builder)
        {
            var aglGraph = new Graph();

            foreach (var buildNode in builder.Nodes)
            {
                string id = this.GetNodeId(buildNode);

                var aglNode = aglGraph.AddNode(id);
                this.DecorateNode(aglNode, buildNode);
            }

            // Add the edges once all the nodes are in the graph
            foreach (var buildNode in builder.Nodes)
            {
                string idFrom = this.GetNodeId(buildNode);

                foreach (var buildEdge in buildNode.OutgoingEdges)
                {
                    string idTo = this.GetNodeId(buildEdge.To);

                    var aglEdge = aglGraph.AddEdge(idFrom, idTo);
                    this.DecorateEdge(aglEdge, buildEdge);
                }
            }

            return aglGraph;
        }

        private string GetNodeId(BuildNode buildNode)
        {
            // Every node in the graph must be on a different position in the code
            return buildNode.Syntax.FullSpan.ToString();
        }

        private void DecorateNode(Node aglNode, BuildNode buildNode)
        {
            var label = new Label();

            label.Text = buildNode.Syntax.ToFullString();

            aglNode.Label = label;
        }

        private void DecorateEdge(Edge aglEdge, BuildEdge buildEdge)
        {
            if (buildEdge.ValueCondition == null)
            {
                return;
            }

            aglEdge.LabelText = buildEdge.ValueCondition.ToString();
        }
    }
}
