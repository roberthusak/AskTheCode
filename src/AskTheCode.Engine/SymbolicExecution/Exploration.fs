namespace AskTheCode.SymbolicExecution

open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.Heap

module Exploration =
    open AskTheCode

    // Definition of functions for heap handling and default value

    type HeapFunctions<'heap> = { GetEmpty: unit -> 'heap; PerformOp: HeapOperation -> 'heap -> 'heap * Term option }

    let unsupportedHeapFn : HeapFunctions<unit> = { GetEmpty = id; PerformOp = (fun _ _ -> failwith "Heap unsupported") }

    // Definition of functions for path condition handling and the simplest implementation based on solver function

    type ConditionFunctions<'condition> = { GetEmpty: unit -> 'condition; Assert: Term -> 'condition -> 'condition; Solve: 'condition -> SolveResult }

    let solverCondFn solver :ConditionFunctions<Term> = { GetEmpty = (fun () -> BoolConst true); Assert = Utils.curry2 And; Solve = solver }

    let assertCondition condFn cond term =
        match term with
        | BoolConst true -> cond
        | _ -> condFn.Assert term cond

    // Variable version handling

    let getVersion versions name =
        Map.tryFind name versions |> Option.defaultValue 0

    let formatVersioned name version =
        sprintf "%s!%d" name version

    let addVersion versions (variable:Variable) =
        let version = getVersion versions variable.Name
        { variable with Name = formatVersioned variable.Name version }

    let rec addVersions versions term =
        match term with
        | Var v ->
            Var <| addVersion versions v
        | _ ->
            Term.updateChildren (addVersions versions) term

    let addHeapOpVersions versions heapOp =
        match heapOp with
        | AssignEquals (trg, left, right) ->
            AssignEquals (addVersion versions trg, left, right)
        | AssignNotEquals (trg, left, right) ->
            AssignNotEquals (addVersion versions trg, left, right)
        | ReadVal (trg, ins, field) ->
            ReadVal (addVersion versions trg, ins, field)
        | WriteVal (ins, field, value) ->
            WriteVal (ins, field, addVersions versions value)
        | _ ->
            heapOp

    // Processing operations into path constraints, version changes and heap updates

    let processOperation heapFn op versions heap =
        match op with
        | (Assign assign) ->
            let trgName = assign.Target.Name
            let trgVersion = getVersion versions trgName
            let target = Var { assign.Target with Name = formatVersioned trgName trgVersion }
            let versions = Map.add trgName (trgVersion + 1) versions
            let value = addVersions versions assign.Value
            (Some (Eq (target, value)), versions, heap)
        | (HeapOp heapOp) ->
            let versionedHeapOp = addHeapOpVersions versions heapOp
            let (heap, heapCond) = heapFn.PerformOp versionedHeapOp heap
            let versions =
                match HeapOperation.targetVariable heapOp with
                | Some { Name = varName } ->
                    let curVersion = getVersion versions varName
                    Map.add varName (curVersion + 1) versions
                | None ->
                    versions
            (heapCond, versions, heap)

    let processNode heapFn node versions heap =
        let folder op (cond, versions, heap) =
            let (opCond, versions, heap) = processOperation heapFn op versions heap
            (Utils.mergeOptions (Utils.curry2 And) cond opCond, versions, heap)
        match node with
        | Basic (_, operations) ->
            List.foldBack folder operations (None, versions, heap)
        | _ ->
            (None, versions, heap)

    // Explore each path separately

    type ExplorerState<'condition, 'heap> = { Path: Path; Condition: 'condition; Versions: Map<string, int>; Heap: 'heap }

    let run condFn heapFn graph targetNode =
        let extend graph state (edge:Edge) =
            match edge with
            | Inner innerEdge ->
                let node = Graph.node graph innerEdge.From
                let path = Step (node, edge, state.Path)
                let cond =
                    addVersions state.Versions innerEdge.Condition
                    |> assertCondition condFn state.Condition
                let (nodeCond, versions, heap) = processNode heapFn node state.Versions state.Heap
                let cond = Option.fold (assertCondition condFn) cond nodeCond
                { state with Path = path; Condition = cond; Versions = versions; Heap = heap }
            | Outer _ ->
                failwith "Not implemented"

        let rec step states results =
            match states with
            | [] ->
                results
            | state :: states' ->
                let node = Path.node state.Path
                match condFn.Solve state.Condition with
                | Sat _ ->
                    match node with
                    | Enter _ ->
                        step states' (state.Path :: results)
                    | _ ->
                        let states'' =
                            Graph.edgesTo graph node
                            |> List.map (Inner >> extend graph state)
                            |> (fun addedStates -> List.append addedStates states')
                        step states'' results
                | _ ->
                    step states' results
        let states = [ { Path = Target targetNode; Condition = condFn.GetEmpty(); Versions = Map.empty; Heap = heapFn.GetEmpty() } ]
        step states []
        