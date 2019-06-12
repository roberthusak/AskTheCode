module AskTheCode.Program

open System.Linq
open System.Collections.Generic
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.FlowAnalysis

open AskTheCode
open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.SymbolicExecution
open AskTheCode.Cli
open AskTheCode.Gui
open System.Diagnostics

let swapNodeSource = @"
using System.Diagnostics;

public class Node
{
    public int value;
    public Node next;

    public Node SwapNode()
    {
        Node result = this;
        if (this.next != null)
        {
            if (this.value > this.next.value)
            {
                Node t = this.next;
                this.next = t.next;
                t.next = this;
                result = t;
            }
        }

        return result;
    }
}"

let degreeCountingSource = @"
using System;

public class DegreeCounting
{
    public class Node
    {
        public Node next;
        public Node pointsTo;

        public int index;
        public int inDegree;
    }

    public int Compute(Node first)
    {
        Node n = first;
        int index = 0;
        while (n != null)
        {
            n.inDegree = 0;
            n.index = index;

            n = n.next;
            index = index + 1;
        }

        n = first;
        while (n != null)
        {
            if (n.pointsTo != null)
            {
                n.pointsTo.inDegree = n.pointsTo.inDegree + 1;
            }

            n = n.next;
        }

        n = first;
        while (n != null)
        {
            if (n.index > 1 && n.inDegree > n.index)
            {
                return 1;
            }

            n = n.next;
        }

        return 0;
    }
}"

let absoluteValueSource = @"
public class C
{
    public static int AbsoluteValue(int x)
    {
        int y;
        if (x >= 0)
        {
            y = x;
        }
        else
        {
            y = -x;
        }

        if (y < 0)
        {
            return 1;
        }

        return 0;
    }
}"

let absoluteValue2source = @"
public class C
{
    public static int AbsoluteValue(int x)
    {
        x = x + 1;
        if (x >= 0)
        {
        }
        else
        {
            x = -x;
        }

        if (x < 0)
        {
            return 1;
        }

        return 0;
    }
}"

let sourceToCfg (source:string) =
    let parseOptions = CSharpParseOptions.Default.WithFeatures([ new KeyValuePair<string, string>("flow-analysis", "flow-analysis") ])
    let syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions)
    let reference = MetadataReference.CreateFromFile(typeof<System.Object>.Assembly.Location)
    let compilation = CSharpCompilation.Create("test", [ syntaxTree ], [ reference ])
    let syntaxNode = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single()
    let roslynCfg = ControlFlowGraph.Create(syntaxNode, compilation.GetSemanticModel(syntaxTree, true))
    Cfg.convertCfg roslynCfg

[<EntryPoint>]
let main args =

    //let (cfg, target) = (sourceToCfg swapNodeSource, NodeId 4)
    let (cfg, target) = (sourceToCfg degreeCountingSource, NodeId 13)
    //let (cfg, target) = (sourceToCfg absoluteValueSource, NodeId 5)
    //let (cfg, target) = (sourceToCfg absoluteValue2source, NodeId 4)

    printfn "%s" <| Graph.print cfg
    cfg |> Msagl.convertCfg |> Msagl.displayGraph

    let cfg = Graph.unwindLoops cfg 3
    printfn "%s" <| Graph.print cfg
    cfg |> Msagl.convertCfg |> Msagl.displayGraph

    let ctx = Z3.mkContext()
    let mutable solveCount = 0;
    let solverWatch = new Stopwatch()
    let solver term =
        printfn "%s" <| Term.print term
        solveCount <- solveCount + 1
        solverWatch.Start()
        let result = Z3.solve ctx term
        solverWatch.Stop()
        printfn "%A" result
        printfn ""
        result

    let wholeWatch = Stopwatch.StartNew()

    let condFn = (Exploration.solverCondFn solver)
    //let condFn = Z3.stackCondFn ctx
    let res = Exploration.run condFn ArrayHeap.functions cfg (Graph.node cfg target)

    wholeWatch.Stop()

    printfn "Total time: %d ms" wholeWatch.ElapsedMilliseconds
    printfn "SMT time: %d ms" solverWatch.ElapsedMilliseconds
    printfn "SMT calls: %d" solveCount
    printfn "Average SMT call: %f ms" <| (float32)solverWatch.ElapsedMilliseconds / (float32)solveCount
    printfn "Time spent on SMT calls: %f %%" <| (float32)100 * (float32)solverWatch.ElapsedMilliseconds / (float32)wholeWatch.ElapsedMilliseconds

    List.iter (Path.print >> printfn "%s") res
    0
