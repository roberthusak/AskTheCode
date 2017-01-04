using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Cli;
using AskTheCode.ControlFlowGraphs.Cli.TypeModels;
using AskTheCode.SmtLibStandard;
using Microsoft.Msagl.Drawing;

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
            return buildNode.Id.Value.ToString();
        }

        private void DecorateNode(Node aglNode, BuildNode buildNode, GraphDepth depth)
        {
            var label = new Label();

            var text = new StringBuilder(buildNode.Label.ToString());

            if (depth == GraphDepth.Value
                && (buildNode.VariableModel != null || buildNode.ValueModel != null || buildNode.BorderData != null))
            {
                text.AppendLine();
                text.Append("[ ");

                if (buildNode.VariableModel != null)
                {
                    if (buildNode.VariableModel.AssignmentLeft.Count == 1)
                    {
                        text.Append(buildNode.VariableModel.AssignmentLeft.Single());
                    }
                    else
                    {
                        text.Append($"({string.Join(", ", buildNode.VariableModel.AssignmentLeft)})");
                    }
                }

                var borderKind = buildNode.BorderData?.Kind;
                if (borderKind == null || borderKind == BorderDataKind.MethodCall)
                {
                    text.Append(" \u2190 ");
                }

                if (buildNode.BorderData != null)
                {
                    switch (buildNode.BorderData.Kind)
                    {
                        case BorderDataKind.Enter:
                            text.Append("enter");
                            break;

                        case BorderDataKind.Return:
                            text.Append("return");
                            if (buildNode.ValueModel != null)
                            {
                                text.Append(' ');
                            }

                            break;

                        case BorderDataKind.MethodCall:
                            Contract.Assert(buildNode.ValueModel == null);

                            var method = buildNode.BorderData.Method;
                            string argumentsText = FormatTypeModelList(buildNode.BorderData.Arguments);
                            text.Append($"{method.ContainingType}.{method.Name}({argumentsText})");
                            break;

                        case BorderDataKind.ExceptionThrow:
                        default:
                            Contract.Assert(buildNode.BorderData.Kind == BorderDataKind.ExceptionThrow);
                            Contract.Assert(buildNode.ValueModel == null);

                            var exceptionConstructor = buildNode.BorderData.Method;
                            string constructorArgumentsText = FormatTypeModelList(buildNode.BorderData.Arguments);
                            text.Append(
                                $"throw {exceptionConstructor.ContainingType.Name}({constructorArgumentsText})");
                            break;
                    }
                }

                if (buildNode.ValueModel != null)
                {
                    if (buildNode.ValueModel.AssignmentRight.Count == 1)
                    {
                        text.Append(buildNode.ValueModel.AssignmentRight.Single());
                    }
                    else
                    {
                        text.Append($"({string.Join(", ", buildNode.ValueModel.AssignmentRight)})");
                    }
                }

                text.Append(" ]");
            }

            label.Text = text.ToString();

            aglNode.Label = label;
        }

        private string FormatTypeModelList(IEnumerable<ITypeModel> typeModels)
        {
            var arguments = typeModels
                .SelectMany(arg => arg?.AssignmentRight)
                .Select(expr => expr?.ToString() ?? "?");
            return string.Join(", ", arguments);
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
