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
static class C
{
    public static int M(int x, int y)
    {
        x = -x;
        if (x > 0) {
            return 1;
        } else if (y < 0) {
            return -1;
        } else {
            return 0;
        }
    }
}"
    let parseOptions = CSharpParseOptions.Default.WithFeatures([ new KeyValuePair<string, string>("flow-analysis", "flow-analysis") ])
    let syntaxTree = CSharpSyntaxTree.ParseText(source, parseOptions)
    let reference = MetadataReference.CreateFromFile(typeof<System.Object>.Assembly.Location)
    let compilation = CSharpCompilation.Create("test", [ syntaxTree ], [ reference ])
    let syntaxNode = syntaxTree.GetRoot().DescendantNodes().OfType<MethodDeclarationSyntax>().Single();
    let roslynCfg = ControlFlowGraph.Create(syntaxNode, compilation.GetSemanticModel(syntaxTree, true))
    let cfg = Cfg.convertCfg roslynCfg
    
    let x = { Sort = Int; Name = "x" }
    let y = { Sort = Int; Name = "y" }
    let refCfg = {
        Nodes = [
            Enter (NodeId 0);
            Basic (NodeId 1, [ Assign { Target = x; Value = Neg (Var x) } ]);
            Return (NodeId 2, Some <| IntConst 1 );
            Basic (NodeId 3, []);
            Return (NodeId 4, Some <| Neg (IntConst 1));
            Return (NodeId 5, Some <| IntConst 0)
        ];
        Edges = [
            { From = NodeId 0; To = NodeId 1; Condition = BoolConst true };
            { From = NodeId 1; To = NodeId 2; Condition = Gt (Var x, IntConst 0) };
            { From = NodeId 1; To = NodeId 3; Condition = Neg (Gt (Var x, IntConst 0)) };
            { From = NodeId 3; To = NodeId 4; Condition = Lt (Var y, IntConst 0) };
            { From = NodeId 3; To = NodeId 5; Condition = Neg (Lt (Var y, IntConst 0)) }
        ]
    }
    assert (cfg = refCfg)

    let ctx = Z3.mkContext()
    let res = Exploration.run (Z3.solve ctx) cfg (Graph.node cfg (NodeId 5))
    System.Console.WriteLine(res.ToString())
    0
