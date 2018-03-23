using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.ControlFlowGraphs.Operations;
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
            builder.AddEdge(enterNode, retNode);

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

            builder.AddEdge(enterNode, retNode);

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
            var checkNode = builder.AddInnerNode(new Assignment(ifResult, a > b));

            var retANode = builder.AddReturnNode(new[] { aParam });
            var retBNode = builder.AddReturnNode(new[] { bParam });

            builder.AddEdge(enterNode, checkNode);
            builder.AddEdge(checkNode, retANode, ifResultHandle);
            builder.AddEdge(checkNode, retBNode, !ifResultHandle);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph ComplexExampleGraph()
        {
            var builder = new FlowGraphBuilder(GetId());

            var aVar = builder.AddLocalVariable(Sort.Int, "a");
            var a = (IntHandle)aVar;
            var bVar = builder.AddLocalVariable(Sort.Int, "b");
            var b = (IntHandle)bVar;
            var cVar = builder.AddLocalVariable(Sort.Bool, "c");
            var c = (BoolHandle)cVar;
            var enterNode = builder.AddEnterNode(new[] { aVar, bVar, cVar });

            var if1Var = builder.AddLocalVariable(Sort.Bool, "if!1");
            var if1 = (BoolHandle)if1Var;
            var if1Node = builder.AddInnerNode(new Assignment(if1Var, a == 0));

            var ret1Node = builder.AddReturnNode(new[] { ExpressionFactory.IntInterpretation(-1) });

            var isNiceNumberLocation = new TestLocation("IsNiceNumber");
            var if21Var = builder.AddLocalVariable(Sort.Bool, "if!2!1");
            var if21 = (BoolHandle)if21Var;
            var if21Node = builder.AddCallNode(isNiceNumberLocation, new[] { aVar }, new[] { if21Var });

            var if22CheckVar = builder.AddLocalVariable(Sort.Bool, "if!2!2!check");
            var if22Check = (BoolHandle)if22CheckVar;
            var if22CheckNode = builder.AddInnerNode(new Assignment(if22CheckVar, b != 0));

            var exceptionLocation = new TestLocation("DivisionByZeroException");
            var if22ThrowNode = builder.AddThrowExceptionNode(exceptionLocation);

            var if22Var = builder.AddLocalVariable(Sort.Bool, "if!2!2");
            var if22 = (BoolHandle)if22Var;
            var if22Node = builder.AddInnerNode(new Assignment(if22Var, a / b > 2 && b != -1));

            var dVar = builder.AddLocalVariable(Sort.Int, "d");
            var d = (IntHandle)dVar;
            var dEqualsBNode = builder.AddInnerNode(new Assignment(dVar, bVar));

            var while1Var = builder.AddLocalVariable(Sort.Bool, "while!1");
            var while1 = (BoolHandle)while1Var;
            var while1Node = builder.AddInnerNode(new Assignment(while1Var, a < b));

            var assert1Var = builder.AddLocalVariable(Sort.Bool, "assert!1");
            var assert1 = (BoolHandle)assert1Var;
            var whileBodyNode = builder.AddInnerNode(
                new[]
                {
                    new Assignment(aVar, a + a),
                    new Assignment(assert1Var, a != 0)
                });

            var elseBodyNode = builder.AddInnerNode(new Assignment(aVar, a + b));

            var ret2Node = builder.AddReturnNode(new[] { aVar });

            builder.AddEdge(enterNode, if1Node);
            builder.AddEdge(if1Node, ret1Node, if1);
            builder.AddEdge(if1Node, if21Node, !if1);
            builder.AddEdge(if21Node, if22CheckNode, if21);
            builder.AddEdge(if22CheckNode, if22ThrowNode, !if22Check);
            builder.AddEdge(if22CheckNode, if22Node, if22Check);
            builder.AddEdge(if22Node, dEqualsBNode, if22);
            builder.AddEdge(dEqualsBNode, while1Node);
            builder.AddEdge(while1Node, whileBodyNode, while1);
            builder.AddEdge(whileBodyNode, while1Node);
            builder.AddEdge(while1Node, ret2Node, !while1);
            builder.AddEdge(if21Node, elseBodyNode, !if21);
            builder.AddEdge(if22Node, elseBodyNode, !if22);
            builder.AddEdge(elseBodyNode, ret2Node);

            return builder.FreezeAndReleaseGraph();
        }

        private static FlowGraphId GetId()
        {
            return new FlowGraphId(0);
        }
    }
}
