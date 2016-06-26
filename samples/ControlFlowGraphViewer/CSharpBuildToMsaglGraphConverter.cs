using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.SmtLibStandard;
using Microsoft.Msagl.Drawing;
using System.Diagnostics.Contracts;

namespace ControlFlowGraphViewer
{
    internal class CSharpBuildToMsaglGraphConverter
    {
        public Graph Convert(BuildGraph buildGraph, GraphDepth depth)
        {
            var aglGraph = new Graph();

            foreach (var buildNode in buildGraph.Nodes)
            {
                string id = this.GetNodeId(buildNode);

                var aglNode = aglGraph.AddNode(id);
                this.DecorateNode(aglNode, buildNode, depth);
            }

            // Add the edges once all the nodes are in the graph
            foreach (var buildNode in buildGraph.Nodes)
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
            return buildNode.Label.FullSpan.ToString();
        }

        private void DecorateNode(Node aglNode, BuildNode buildNode, GraphDepth depth)
        {
            var label = new Label();

            var text = new StringBuilder(buildNode.Label.ToString());

            if (depth == GraphDepth.Value && (buildNode.VariableModel != null || buildNode.ValueModel != null))
            {
                text.AppendLine();
                text.Append("[ ");

                if (buildNode.VariableModel != null)
                {
                    text.Append($"({string.Join(", ", buildNode.VariableModel.AssignmentLeft)})");
                }

                text.Append(" \u2190 ");

                if (buildNode.ValueModel != null)
                {
                    text.Append($"({string.Join(", ", buildNode.ValueModel.AssignmentRight)})");
                }

                if (buildNode.BorderData != null)
                {
                    Contract.Assert(buildNode.ValueModel == null);

                    var method = buildNode.BorderData.Method;
                    var arguments = buildNode.BorderData.Arguments.SelectMany(arg => arg.AssignmentRight);
                    string argumentsText = string.Join(", ", arguments);
                    text.Append($"{method.ContainingType}.{method.Name}({argumentsText})");
                }

                text.Append(" ]");
            }

            label.Text = text.ToString();

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
