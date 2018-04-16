using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AskTheCode.Common;
using AskTheCode.ControlFlowGraphs.Heap;
using AskTheCode.ControlFlowGraphs.Operations;
using AskTheCode.SmtLibStandard;
using AskTheCode.SmtLibStandard.Handles;

namespace AskTheCode.ControlFlowGraphs.Tests
{
    public static class SampleFlowGraphGenerator
    {
        public static FlowGraph TrivialGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

            var enterNode = builder.AddEnterNode();
            var retNode = builder.AddReturnNode();
            builder.AddEdge(enterNode, retNode);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph IntAddGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

            var aParam = builder.AddLocalVariable(Sort.Int, "a");
            var bParam = builder.AddLocalVariable(Sort.Int, "b");
            var enterNode = builder.AddEnterNode(new[] { aParam, bParam });

            var retValueHandle = (IntHandle)aParam + (IntHandle)bParam;
            var retNode = builder.AddReturnNode(new[] { retValueHandle.Expression });

            builder.AddEdge(enterNode, retNode);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph IntMaxGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

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

        public static FlowGraph ComplexExampleGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

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

            var isNiceNumberLocation = new TestRoutineLocation("IsNiceNumber");
            var if21Var = builder.AddLocalVariable(Sort.Bool, "if!2!1");
            var if21 = (BoolHandle)if21Var;
            var if21Node = builder.AddCallNode(isNiceNumberLocation, new[] { aVar }, new[] { if21Var });

            var if22CheckVar = builder.AddLocalVariable(Sort.Bool, "if!2!2!check");
            var if22Check = (BoolHandle)if22CheckVar;
            var if22CheckNode = builder.AddInnerNode(new Assignment(if22CheckVar, b != 0));

            var exceptionLocation = new TestRoutineLocation("DivisionByZeroException");
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

        [GeneratedMethodProperties(IsConstructor = true)]
        public static FlowGraph NodeConstructorGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

            var @this = builder.AddLocalVariable(References.Sort, "this");
            var value = builder.AddLocalVariable(Sort.Int, "value");
            var next = builder.AddLocalVariable(References.Sort, "next");

            var enterNode = builder.AddEnterNode(new[] { @this, value, next });

            var initNode = builder.AddInnerNode(new[]
            {
                new FieldWrite(@this, SampleLinkedListDefinitions.Value, value),
                new FieldWrite(@this, SampleLinkedListDefinitions.Next, next)
            });

            var returnNode = builder.AddReturnNode(@this.ToSingular());

            builder.AddEdge(enterNode, initNode);
            builder.AddEdge(initNode, returnNode);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph HeapSimpleConstructorGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

            var enterNode = builder.AddEnterNode();

            var n = builder.AddLocalVariable(References.Sort, "n");

            var newNode = builder.AddCallNode(
                new TestRoutineLocation(typeof(SampleFlowGraphGenerator).GetMethod(nameof(NodeConstructorGraph)), true),
                new Expression[] { References.Null, ExpressionFactory.IntInterpretation(0), References.Null },
                n.ToSingular(),
                true);

            var n_value = builder.AddLocalVariable(Sort.Int, "n_value");
            var assert1 = builder.AddLocalVariable(Sort.Bool, "assert1");
            var assert2 = builder.AddLocalVariable(Sort.Bool, "assert2");

            var assertNode = builder.AddInnerNode(new Operation[]
            {
                new Assignment(assert1, builder.AddReferenceComparisonVariable(false, n, References.Null)),
                new FieldRead(n_value, n, SampleLinkedListDefinitions.Value),
                new Assignment(assert2, (IntHandle)n_value == 0)
            });

            var returnNode = builder.AddReturnNode();

            builder.AddEdge(enterNode, newNode);
            builder.AddEdge(newNode, assertNode);
            builder.AddEdge(assertNode, returnNode);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph HeapSimpleBranchingGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

            var n = builder.AddLocalVariable(References.Sort, "n");

            var enterNode = builder.AddEnterNode(n.ToSingular());

            var n_eq_null = builder.AddReferenceComparisonVariable(true, n, References.Null);
            var n_neq_null = builder.AddReferenceComparisonVariable(false, n, References.Null);

            var eqNewNode = builder.AddCallNode(
                new TestRoutineLocation(typeof(SampleFlowGraphGenerator).GetMethod(nameof(NodeConstructorGraph)), true),
                new Expression[] { References.Null, ExpressionFactory.IntInterpretation(0), n },
                n.ToSingular(),
                true);

            var n_next = builder.AddLocalVariable(References.Sort, "n_next");
            var eqAssertResult = builder.AddLocalVariable(Sort.Bool, "assert1");

            var eqAssertNode = builder.AddInnerNode(new Operation[]
            {
                new FieldRead(n_next, n, SampleLinkedListDefinitions.Next),
                new Assignment(eqAssertResult, builder.AddReferenceComparisonVariable(true, n_next, References.Null))
            });

            var val = builder.AddLocalVariable(Sort.Int, "val");
            var neqAssertResult = builder.AddLocalVariable(Sort.Bool, "assert2");

            var neqNode = builder.AddInnerNode(new Operation[]
            {
                new FieldRead(val, n, SampleLinkedListDefinitions.Value),
                new Assignment(neqAssertResult, builder.AddReferenceComparisonVariable(false, n, References.Null))
            });

            var returnNode = builder.AddReturnNode();

            builder.AddEdge(enterNode, eqNewNode, n_eq_null);
            builder.AddEdge(eqNewNode, eqAssertNode);
            builder.AddEdge(eqAssertNode, returnNode);
            builder.AddEdge(enterNode, neqNode, n_neq_null);
            builder.AddEdge(neqNode, returnNode);

            return builder.FreezeAndReleaseGraph();
        }

        public static FlowGraph HeapSimpleComparisonGraph(FlowGraphId id)
        {
            var builder = new FlowGraphBuilder(id);

            var a = builder.AddLocalVariable(References.Sort, "a");
            var b = builder.AddLocalVariable(References.Sort, "b");

            var enterNode = builder.AddEnterNode(new[] { a, b });

            var a_eq_b = builder.AddReferenceComparisonVariable(true, a, b);
            var a_neq_b = builder.AddReferenceComparisonVariable(false, a, b);

            var a_next = builder.AddLocalVariable(References.Sort, "a_next");
            var b_next = builder.AddLocalVariable(References.Sort, "b_next");
            var a_next_eq_b_next = builder.AddReferenceComparisonVariable(true, a_next, b_next);
            var eqAssert = builder.AddLocalVariable(Sort.Bool, "assert1");

            var eqNode = builder.AddInnerNode(new Operation[]
            {
                new FieldRead(a_next, a, SampleLinkedListDefinitions.Next),
                new FieldRead(b_next, b, SampleLinkedListDefinitions.Next),
                new Assignment(eqAssert, a_next_eq_b_next)
            });

            var a_value = builder.AddLocalVariable(Sort.Int, "a_value");
            var b_value = builder.AddLocalVariable(Sort.Int, "b_value");
            var neqAssert = builder.AddLocalVariable(Sort.Bool, "assert2");

            var neqNode = builder.AddInnerNode(new Operation[]
            {
                new FieldWrite(a, SampleLinkedListDefinitions.Value, new IntHandle(5)),
                new FieldWrite(b, SampleLinkedListDefinitions.Value, new IntHandle(10)),

                new FieldRead(a_value, a, SampleLinkedListDefinitions.Value),
                new FieldRead(b_value, b, SampleLinkedListDefinitions.Value),
                new Assignment(neqAssert, (IntHandle)a_value != (IntHandle)b_value)
            });

            var returnNode = builder.AddReturnNode();

            builder.AddEdge(enterNode, eqNode, a_eq_b);
            builder.AddEdge(eqNode, returnNode);
            builder.AddEdge(enterNode, neqNode, a_neq_b);
            builder.AddEdge(neqNode, returnNode);

            return builder.FreezeAndReleaseGraph();
        }
    }
}
