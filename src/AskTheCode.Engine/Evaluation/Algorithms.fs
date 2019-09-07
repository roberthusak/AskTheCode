module AskTheCode.Evaluation.Algorithms

open AskTheCode
open AskTheCode.Cfg
open AskTheCode.SymbolicExecution
open AskTheCode.Evaluation.Samples

type Algorithm =
    {
        Name: string;
        Function: (NodeId -> bool) -> Graph -> Node -> Path list;
        GetDoSolve: Sample -> (NodeId -> bool);
    }

let mergeFun =
    use ctx = Z3.mkContext()
    let condFn = Z3.stackCondFn ctx
    Exploration.mergeRun condFn ArrayHeap.functions

let wpFun wpVariant =
    use ctx = Z3.mkContext()
    Exploration.wpRun (wpVariant ctx) ArrayReplacementHeap.functions

let neverSolve _ = false
let alwaysSolve _ = true
let setSolve idSet id = Set.contains id idSet

let getNeverSolve _ = neverSolve
let getAlwaysSolve _ = alwaysSolve
let getLoopStartSolve sample = setSolve <| Set.ofList sample.LoopStarts

let mergeNever = { Name = "Merge - never solve"; Function = mergeFun; GetDoSolve = getNeverSolve }
let mergeAlways = { Name = "Merge - always solve"; Function = mergeFun; GetDoSolve = getAlwaysSolve }
let mergeLoops = { Name = "Merge - solve loop starts"; Function = mergeFun; GetDoSolve = getLoopStartSolve }

let wpTerm = { Name = "WP - single term"; Function = wpFun Z3.wpTermFn; GetDoSolve = getNeverSolve }
let wpCombination = { Name = "WP - combination"; Function = wpFun Z3.wpCombFn; GetDoSolve = getNeverSolve }
let wpSet = { Name = "WP - disjunct set"; Function = wpFun Z3.wpSetFn; GetDoSolve = getNeverSolve }
