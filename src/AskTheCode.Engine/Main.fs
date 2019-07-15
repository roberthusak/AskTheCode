module AskTheCode.Program

open AskTheCode
open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.SymbolicExecution
open AskTheCode.Gui
open System.Diagnostics

[<EntryPoint>]
let main args =

    let sample = Samples.degreeCounting()
    let cfg = sample.Cfg

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
    let res = Exploration.run condFn ArrayHeap.functions cfg (Graph.node cfg sample.TargetNode)
    //let res = Exploration.mergeRun condFn ArrayHeap.functions cfg (Graph.node cfg sample.TargetNode)

    wholeWatch.Stop()

    printfn "Total time: %d ms" wholeWatch.ElapsedMilliseconds
    printfn "SMT time: %d ms" solverWatch.ElapsedMilliseconds
    printfn "SMT calls: %d" solveCount
    printfn "Average SMT call: %f ms" <| (float32)solverWatch.ElapsedMilliseconds / (float32)solveCount
    printfn "Time spent on SMT calls: %f %%" <| (float32)100 * (float32)solverWatch.ElapsedMilliseconds / (float32)wholeWatch.ElapsedMilliseconds

    List.iter (Path.print >> printfn "%s") res
    0
