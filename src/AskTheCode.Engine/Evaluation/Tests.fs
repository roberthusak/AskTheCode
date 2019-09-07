module AskTheCode.Evaluation.Tests

open System.Diagnostics

open AskTheCode.Cfg
open AskTheCode.SymbolicExecution
open AskTheCode.Evaluation.Samples
open AskTheCode.Evaluation.Algorithms

let efficiency sample algorithm =
    let trgNode = Graph.node sample.Cfg sample.TargetNode
    let doSolve = algorithm.GetDoSolve sample
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

let exportLatex discipline (samples:seq<Sample>) algorithms =
    // Table header
    printfn @"\hline"
    algorithms
    |> Seq.map (fun alg -> alg.Name)
    |> String.concat " & "
    |> printfn @"Test Case & %s \\"
    printfn @"\hline"
    // Rows
    for sample in samples do
        algorithms
        |> Seq.map (discipline sample >> sprintf "%d")
        |> String.concat " & "
        |> printfn @"\textit{%s} & %s \\" sample.Name
        printfn @"\hline"

let compareDegreeCounting () =
    let samples = seq {
        for i in 1..4 do
        for j in -1..1 do
        yield Samples.degreeCounting (i+j) (i+2) 
    }
    let algorithms =
        [
            Algorithms.wpSet;
            Algorithms.wpTerm;
            Algorithms.wpCombination;
            Algorithms.mergeNever;
            Algorithms.mergeAlways;
            Algorithms.mergeLoops
        ]
    compare efficiency printer samples algorithms
