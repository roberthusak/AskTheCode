module AskTheCode.Program

open AskTheCode
open AskTheCode.Cfg

open System.Linq
open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.CSharp.Syntax
open Microsoft.CodeAnalysis.FlowAnalysis
open System.Collections.Generic

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
    let cfg = Cfg.CSharp.convertCfg roslynCfg
    
    let x = { Sort = Int; Name = "x" }
    let y = { Sort = Int; Name = "y" }
    let refCfg = {
        Nodes = [
            Enter (NodeId 0);
            Inner (NodeId 1, { Operations = [ Assign { Target = x; Value = Neg (Var x) } ] });
            Return (NodeId 2, { Value = IntConst 1 });
            Inner (NodeId 3, { Operations = [] });
            Return (NodeId 4, { Value = Neg (IntConst 1) });
            Return (NodeId 5, { Value = IntConst 0 })
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

    let ctx = new Microsoft.Z3.Context()

    let res = Exploration.run (Z3.solve ctx) cfg (Graph.node cfg (NodeId 5))
    System.Console.WriteLine(res.ToString())
    0