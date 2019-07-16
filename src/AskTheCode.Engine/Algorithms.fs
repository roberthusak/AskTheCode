module AskTheCode.Algorithms

open AskTheCode.Cfg
open AskTheCode.SymbolicExecution

type Algorithm = { Name: string; Function: Graph -> Node -> Path list }

let mergeFun =
    use ctx = Z3.mkContext()
    let condFn = Exploration.solverCondFn <| Z3.solve ctx
    Exploration.mergeRun condFn ArrayHeap.functions

let wpFun wpVariant =
    use ctx = Z3.mkContext()
    Exploration.wpRun (wpVariant ctx) ArrayReplacementHeap.functions

let merge = { Name = "Merge"; Function = mergeFun }

let wpTerm = { Name = "WP - single term"; Function = wpFun Z3.wpTermFn }
let wpCombination = { Name = "WP - combination"; Function = wpFun Z3.wpCombFn }
let wpSet = { Name = "WP - disjunct set"; Function = wpFun Z3.wpSetFn }
