module AskTheCode.Evaluation.Tests

open System.Diagnostics

open AskTheCode.Cfg
open AskTheCode.SymbolicExecution
open AskTheCode.Evaluation.Samples
open AskTheCode.Evaluation.Algorithms

let neverSolve _ = false
let alwaysSolve _ = true
let setSolve idSet id = Set.contains id idSet

let efficiency sample algorithm =
    let trgNode = Graph.node sample.Cfg sample.TargetNode
    let doSolve = setSolve <| Set.ofList sample.LoopStarts
    let run () = algorithm.Function doSolve sample.Cfg trgNode

    let res = run()

    match (res, sample.IsValid) with
    | (h::t, true) -> printfn "ERROR: The counterexample shouldn't have been found (%s, %s)." sample.Name algorithm.Name
    | ([], false) -> printfn "ERROR: The counterexample should have been found (%s, %s)." sample.Name algorithm.Name
    | _ -> ()

    let repeats :int64 = 10L
    let wholeWatch = Stopwatch.StartNew()
    for i in 1L..repeats do
        run() |> ignore
    wholeWatch.Stop()

    wholeWatch.ElapsedMilliseconds / repeats

let compare discipline printer samples algorithms =
    for algorithm in algorithms do
        printfn "%s" algorithm.Name
        for sample in samples do
            discipline sample algorithm |> printer sample algorithm

let printer (sample:Sample) algorithm ms = printfn "%s: %d ms" sample.Name ms

let compareDegreeCounting () =
    let samples = seq {
        for i in 1..4 do
        for j in -1..1 do
        yield Samples.degreeCounting (i+j) (i+2) 
    }
    let algorithms = [ Algorithms.merge; Algorithms.wpTerm; Algorithms.wpCombination; Algorithms.wpSet ]
    compare efficiency printer samples algorithms
