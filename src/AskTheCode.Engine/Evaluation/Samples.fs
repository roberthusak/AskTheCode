module AskTheCode.Evaluation.Samples

open System.Linq
open System.Collections.Generic
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.FlowAnalysis

open AskTheCode
open AskTheCode.Cfg
open AskTheCode.Cli

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

let swapNodeSource2 = @"
using System.Diagnostics;

public class Node
{
    public int value;
    public Node next;

    public int SwapNode()
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

            if (this.value > this.next.value)
            {
                return 1;
            }
        }

        return 0;
    }
}"

let nodeMaxSource = @"
public class Node
{
    public int value;
    public Node next;

    public static int Max(Node a, Node b)
    {
        Node m;
        if (a.value >= b.value)
        {
            m = a;
        }
        else
        {
            m = b;
        }

        if (m.value < a.value || m.value < b.value)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}"

let nodeMaxSource2 = @"
public class Node
{
    public int value;
    public Node next;

    public static int Max(Node m, Node b)
    {
        if (b.value > m.value)
        {
            b = b.next;
            m = b;
        }

        if (m.value < b.value)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}"

let degreeCountingSource indexGt =
    sprintf 
        @"using System;

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
            if (n.index > %d && n.inDegree > n.index)
            {
                return 1;
            }

            n = n.next;
        }

        return 0;
    }
}"
        indexGt

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

type Sample = { Name: string; Cfg: Graph; TargetNode: NodeId; IsValid: bool; LoopStarts: NodeId list }

let nodeSwap1 () = { Name = "Node swap #1"; Cfg = sourceToCfg swapNodeSource; TargetNode = NodeId 4; IsValid = false; LoopStarts = [] }
let nodeSwap2 () = { Name = "Node swap #2"; Cfg = sourceToCfg swapNodeSource2; TargetNode = NodeId 5; IsValid = true; LoopStarts = [] }
let nodeMax1 () = { Name = "Node max #1"; Cfg = sourceToCfg nodeMaxSource; TargetNode = NodeId 6; IsValid = true; LoopStarts = [] }
let nodeMax2 () = { Name = "Node max #2"; Cfg = sourceToCfg nodeMaxSource2; TargetNode = NodeId 4; IsValid = false; LoopStarts = [] }
let degreeCounting indexGt loopUnwind =
    {
        Name = sprintf "Degree counting (%d, %d)" indexGt loopUnwind;
        Cfg = degreeCountingSource indexGt |> sourceToCfg |> Graph.unwindLoops loopUnwind;
        TargetNode = NodeId 13;
        IsValid = indexGt + 2 > loopUnwind;
        LoopStarts = [ NodeId 4; NodeId 9 ]
    }
let absoluteValue1 () = { Name = "Absolute value #1"; Cfg = sourceToCfg absoluteValueSource; TargetNode = NodeId 5; IsValid = true; LoopStarts = [] }
let absoluteValue2 () = { Name = "Absolute value #2"; Cfg = sourceToCfg absoluteValue2source; TargetNode = NodeId 4; IsValid = true; LoopStarts = [] }
