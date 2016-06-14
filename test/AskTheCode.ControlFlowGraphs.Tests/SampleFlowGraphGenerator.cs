using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public static class SampleFlowGraphGenerator
    {
        public static FlowGraph TrivialGraph()
        {
            var builder = new FlowGraphBuilder(GetId());

            var enterNode = builder.AddEnterNode();
            var retNode = builder.AddReturnNode();
            builder.AddEdge(enterNode, retNode, true);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph IntAddGraph()
        {
            var builder = new FlowGraphBuilder(GetId());

            var aParam = builder.AddLocalVariable(Sort.Int, "a");
            var bParam = builder.AddLocalVariable(Sort.Int, "b");
            var enterNode = builder.AddEnterNode(new[] { aParam, bParam });

            var retValueHandle = (IntHandle)aParam + (IntHandle)bParam;
            var retNode = builder.AddReturnNode(new[] { retValueHandle.Expression });

            builder.AddEdge(enterNode, retNode, true);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph IntMaxGraph()
        {
            var builder = new FlowGraphBuilder(GetId());

            var aParam = builder.AddLocalVariable(Sort.Int, "a");
            var a = (IntHandle)aParam;
            var bParam = builder.AddLocalVariable(Sort.Int, "b");
            var b = (IntHandle)bParam;
            var enterNode = builder.AddEnterNode(new[] { aParam, bParam });

            var ifResult = builder.AddLocalVariable(Sort.Bool, "ifResult");
            var ifResultHandle = (BoolHandle)ifResult;
            var checkNode = builder.AddInnerNode(new[] { new Assignment(ifResult, a > b) });

            var retANode = builder.AddReturnNode(new[] { aParam });
            var retBNode = builder.AddReturnNode(new[] { bParam });

            builder.AddEdge(enterNode, checkNode, true);
            builder.AddEdge(checkNode, retANode, ifResultHandle);
            builder.AddEdge(checkNode, retBNode, !ifResultHandle);

            return builder.FreezeAndReleaseGraph();
        }

        private static FlowGraphId GetId()
        {
            return new FlowGraphId(0);
        }
    }
}
