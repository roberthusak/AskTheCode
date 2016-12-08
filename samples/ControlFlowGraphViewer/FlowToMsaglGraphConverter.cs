using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.SmtLibStandard;
using Microsoft.Msagl.Drawing;

namespace ControlFlowGraphViewer
{
    public class FlowToMsaglGraphConverter
    {
        public Graph Convert(FlowGraph flowGraph)
        {
            var aglGraph = new Graph();

            foreach (var flowNode in flowGraph.Nodes)
            {
                var aglNode = aglGraph.AddNode(this.GetNodeId(flowNode));
                this.DecorateNode(aglNode, flowNode);
            }

            foreach (var flowEdge in flowGraph.Edges)
            {
                var aglEdge = aglGraph.AddEdge(
                    this.GetNodeId(flowEdge.From),
                    this.GetNodeId(flowEdge.To));
                this.DecorateEdge(aglEdge, flowEdge);
            }

            return aglGraph;
        }

        private string GetNodeId(FlowNode flowNode)
        {
            return flowNode.Id.Value.ToString();
        }

        private void DecorateNode(Node aglNode, FlowNode flowNode)
        {
            var label = new Label();

            // TODO: Make pretty when the Visitor pattern is implemented
            if (flowNode is EnterFlowNode)
            {
                var enterNode = (EnterFlowNode)flowNode;
                if (enterNode.Parameters.Count > 0)
                {
                    string paramsString = string.Join(", ", enterNode.Parameters);
                    label.Text = $"enter ({paramsString})";
                }
                else if (enterNode.Parameters.Count == 1)
                {
                    label.Text = $"enter {enterNode.Parameters[0]}";
                }
                else
                {
                    label.Text = "enter";
                }
            }
            else if (flowNode is InnerFlowNode)
            {
                var innerNode = (InnerFlowNode)flowNode;
                label.Text = string.Join(
                    "\n",
                    innerNode.Assignments.Select(assignment => $"{assignment.Variable} \u2190 {assignment.Value}"));
            }
            else if (flowNode is CallFlowNode)
            {
                var callNode = (CallFlowNode)flowNode;
                var labelBuild = new StringBuilder();

                if (callNode.ReturnAssignments.Count > 0)
                {
                    labelBuild.Append('(');
                    labelBuild.Append(string.Join(", ", callNode.ReturnAssignments));
                    if (callNode.Location.CanBeExplored)
                    {
                        labelBuild.Append(") \u2190 "); 
                    }
                    else
                    {
                        labelBuild.Append(") ~ ");
                    }
                }

                labelBuild.Append(callNode.Location.ToString());
                labelBuild.Append('(');
                labelBuild.Append(string.Join(", ", callNode.Arguments));
                labelBuild.Append(')');

                label.Text = labelBuild.ToString();
            }
            else if (flowNode is ReturnFlowNode)
            {
                var retNode = (ReturnFlowNode)flowNode;
                if (retNode.ReturnValues.Count > 1)
                {
                    string retsString = string.Join(", ", retNode.ReturnValues);
                    label.Text = $"return ({retsString})";
                }
                else if (retNode.ReturnValues.Count == 1)
                {
                    label.Text = $"return {retNode.ReturnValues[0]}";
                }
                else
                {
                    label.Text = "return";
                }
            }
            else if (flowNode is ThrowExceptionFlowNode)
            {
                var exceptionNode = (ThrowExceptionFlowNode)flowNode;
                string argsString = string.Join(", ", exceptionNode.Arguments);
                label.Text = $"throw {exceptionNode.ConstructorLocation}({argsString})";
            }

            aglNode.Label = label;
        }

        private void DecorateEdge(Edge aglEdge, InnerFlowEdge flowEdge)
        {
            if (flowEdge.Condition.Expression == ExpressionFactory.True)
            {
                return;
            }

            aglEdge.LabelText = flowEdge.Condition.ToString();
        }
    }
}
