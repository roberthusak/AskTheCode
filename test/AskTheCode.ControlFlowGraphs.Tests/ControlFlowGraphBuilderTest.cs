using System;
using AskTheCode.ControlFlowGraphs;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;
using AskTheCode.SmtLibStandard.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    [TestClass]
    public class ControlFlowGraphBuilderTest
    {
        [TestMethod]
        public void EmptyGraphCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);
            Assert.AreNotEqual(null, builder.Graph);
            Assert.AreEqual(true, builder.Graph.CanFreeze);

            var graphHandler = builder.FreezeAndReleaseGraph();
            Assert.AreEqual(null, builder.Graph);
            Assert.AreNotEqual(null, graphHandler.Value);

            FlowGraph graph = graphHandler;

            Assert.AreEqual(graphHandler.Value, graph);
            Assert.AreEqual(true, graph.IsFrozen);
            Assert.AreEqual(graphId, graph.Id);

            Assert.AreEqual(0, graph.Nodes.Count);
            Assert.AreEqual(0, graph.Edges.Count);
            Assert.AreEqual(0, graph.LocalVariables.Count);
        }

        [TestMethod]
        public void EmptyEnterNodeCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            var node = builder.AddEnterNode();
            FlowGraphTestHelper.CheckEnterNode(node, builder.Graph, 0, 0, 0);
        }

        [TestMethod]
        public void EmptyInnerNodeCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            var node = builder.AddInnerNode();
            FlowGraphTestHelper.CheckInnerNode(node, builder.Graph, 0, 0, 0);
        }

        [TestMethod]
        public void EmptyCallNodeCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            var location = new TestLocation(0);

            var node = builder.AddCallNode(location);
            FlowGraphTestHelper.CheckCallNode(node, builder.Graph, 0, 0, location, 0, 0);
        }

        [TestMethod]
        public void EmptyReturnNodeCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            var node = builder.AddReturnNode();
            FlowGraphTestHelper.CheckReturnNode(node, builder.Graph, 0, 0, 0);
        }

        [TestMethod]
        public void LocalVariableCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            string name = "testBoolVar";
            var variable = builder.AddLocalVariable(Sort.Bool, name);

            ExpressionTestHelper.CheckExpression(variable, ExpressionKind.Variable, Sort.Bool, name, 0);
            Assert.AreEqual(builder.Graph, variable.Graph);
        }

        [TestMethod]
        public void GlobalVariableCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            string name = "testBoolVar";
            var variableId = new FlowGraphGlobalVariableId(1);
            var variable = new FlowGraphGlobalVariable(variableId, Sort.Bool, name);

            ExpressionTestHelper.CheckExpression(variable, ExpressionKind.Variable, Sort.Bool, name, 0);
        }

        [TestMethod]
        public void EdgeCreatedProperly()
        {
            var graphId = new FlowGraphId(1);
            var builder = new FlowGraphBuilder(graphId);

            var nodeA = builder.AddInnerNode();
            var nodeB = builder.AddInnerNode();
            var edge = builder.AddEdge(nodeA, nodeB, true);

            Assert.AreEqual(builder.Graph, edge.Graph);
            Assert.AreEqual(nodeA, edge.From);
            Assert.AreEqual(nodeB, edge.To);
            Assert.AreEqual(ExpressionFactory.True, edge.Condition.Expression);
        }
    }
}
