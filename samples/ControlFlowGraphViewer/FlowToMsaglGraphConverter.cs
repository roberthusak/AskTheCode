using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.ControlFlowGraphs.Operations;
using AskTheCode.SmtLibStandard;
using Microsoft.Msagl.Drawing;

namespace ControlFlowGraphViewer
{
    public class FlowToMsaglGraphConverter
    {
        private static OperationToTextConverter operationToText = new OperationToTextConverter();

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
                    innerNode.Operations.Select(operation => operationToText.Visit(operation)));
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

                if (callNode.IsConstructorCall)
                {
                    labelBuild.Append("new ");
                }

                labelBuild.Append(callNode.Location.ToString());
                labelBuild.Append('(');

                if (callNode.IsConstructorCall)
                {
                    labelBuild.Append($"_ ({callNode.Arguments[0]})");
                    if (callNode.Arguments.Count > 1)
                    {
                        labelBuild.Append(", ");
                        labelBuild.Append(string.Join(", ", callNode.Arguments.Skip(1)));
                    }
                }
                else
                {
                    labelBuild.Append(string.Join(", ", callNode.Arguments)); 
                }

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

        private class OperationToTextConverter : OperationVisitor<string>
        {
            public override string VisitAssignment(Assignment assignment)
            {
                return $"{assignment.Variable} \u2190 {assignment.Value}";
            }

            public override string VisitFieldRead(FieldRead fieldRead)
            {
                return $"{fieldRead.ResultStore} \u2190 {fieldRead.Reference}.{fieldRead.Field}";
            }

            public override string VisitFieldWrite(FieldWrite fieldWrite)
            {
                return $"{fieldWrite.Reference}.{fieldWrite.Field} \u2190 {fieldWrite.Value}";
            }
        }
    }
}
