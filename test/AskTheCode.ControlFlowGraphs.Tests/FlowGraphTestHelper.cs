using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    internal static class FlowGraphTestHelper
    {
        internal static void CheckNode(FlowGraphNode node, FlowGraph graph, int ingoingCount, int outgoingCount)
        {
            Assert.AreEqual(graph, node.Graph);
            Assert.AreEqual(graph.IsFrozen, node.IsFrozen);
            Assert.AreEqual(false, node.CanFreeze);

            Assert.AreNotEqual(null, node.IngoingEdges);
            Assert.AreEqual(ingoingCount, node.IngoingEdges.Count);

            Assert.AreNotEqual(null, node.OutgoingEdges);
            Assert.AreEqual(outgoingCount, node.OutgoingEdges.Count);
        }

        internal static void CheckEnterNode(
            FlowGraphEnterNode node,
            FlowGraph graph,
            int ingoingCount,
            int outgoingCount,
            int parametersCount)
        {
            CheckNode(node, graph, ingoingCount, outgoingCount);

            Assert.AreNotEqual(null, node.Parameters);
            Assert.AreEqual(parametersCount, node.Parameters.Count);
        }

        internal static void CheckInnerNode(
            FlowGraphInnerNode node,
            FlowGraph graph,
            int ingoingCount,
            int outgoingCount,
            int assignmentsCount)
        {
            CheckNode(node, graph, ingoingCount, outgoingCount);

            Assert.AreNotEqual(null, node.Assignments);
            Assert.AreEqual(assignmentsCount, node.Assignments.Count);
        }

        internal static void CheckCallNode(
            FlowGraphCallNode node,
            FlowGraph graph,
            int ingoingCount,
            int outgoingCount,
            ILocation location,
            int argumentsCount,
            int returnAssignmentsCount)
        {
            CheckNode(node, graph, ingoingCount, outgoingCount);

            Assert.AreEqual(location, node.Location);

            Assert.AreNotEqual(null, node.Arguments);
            Assert.AreEqual(argumentsCount, node.Arguments.Count);

            Assert.AreNotEqual(null, node.ReturnAssignments);
            Assert.AreEqual(returnAssignmentsCount, node.ReturnAssignments.Count);
        }

        internal static void CheckReturnNode(
            FlowGraphReturnNode node,
            FlowGraph graph,
            int ingoingCount,
            int outgoingCount,
            int returnValuesCount)
        {
            CheckNode(node, graph, ingoingCount, outgoingCount);

            Assert.AreNotEqual(null, node.ReturnValues);
            Assert.AreEqual(returnValuesCount, node.ReturnValues.Count);
        }

        internal static void CheckThrowExceptionNode(
            FlowGraphThrowExceptionNode node,
            FlowGraph graph,
            int ingoingCount,
            int outgoingCount,
            ILocation constructorLocation,
            int argumentsCount)
        {
            CheckNode(node, graph, ingoingCount, outgoingCount);

            Assert.AreEqual(constructorLocation, node.ConstructorLocation);

            Assert.AreNotEqual(null, node.Arguments);
            Assert.AreEqual(argumentsCount, node.Arguments.Count);
        }
    }
}
