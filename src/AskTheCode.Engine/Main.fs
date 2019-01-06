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

[<EntryPoint>]
let main args =
    let source = @"
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
    let parseOptions = CSharpParseOptions.Default.WithFeatures([ new KeyValuePair<string, string>("flow-analysis", "flow-analysis") ])
    let syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions)
    let reference = MetadataReference.CreateFromFile(typeof<System.Object>.Assembly.Location)
    let compilation = CSharpCompilation.Create("test", [ syntaxTree ], [ reference ])
    let syntaxNode = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
    let roslynCfg = ControlFlowGraph.Create(syntaxNode, compilation.GetSemanticModel(syntaxTree, true))
    let cfg = Cfg.convertCfg roslynCfg
    printfn "%A" cfg

    let ctx = Z3.mkContext()
    let solver term =
        printfn "%s" <| Term.print term
        let result = Z3.solve ctx term
        printfn "%A" result
        printfn ""
        result

    let res = Exploration.run ArrayHeap.functions solver cfg (Graph.node cfg (NodeId 4))
    printfn "%A" res
    0
