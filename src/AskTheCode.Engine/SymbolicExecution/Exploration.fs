namespace AskTheCode.SymbolicExecution

open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.Heap

module Exploration =
    open AskTheCode

    type HeapFunctions<'heap> = { GetEmpty: unit -> 'heap; PerformOp: HeapOperation -> 'heap -> 'heap * Term option }

    type ConditionFunctions<'condition> = { GetEmpty: unit -> 'condition; Assert: Term -> 'condition -> 'condition; Solve: 'condition -> SolveResult }

    type State<'condition, 'heap> = { Path: Path; Condition: 'condition; Versions: Map<string, int>; Heap: 'heap }

    let solverCondFn solver :ConditionFunctions<Term> = { GetEmpty = (fun () -> BoolConst true); Assert = Utils.curry2 And; Solve = solver }

    let extend condFn heapFn graph state (edge:Edge) =

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

        let processOperation op state =
            match op with
            | (Assign assign) ->
                let trgName = assign.Target.Name
                let trgVersion = getVersion state.Versions trgName
                let target = Var { assign.Target with Name = formatVersioned trgName trgVersion }
                let versions = Map.add trgName (trgVersion + 1) state.Versions
                let value = addVersions versions assign.Value
                let cond = condFn.Assert (Eq (target, value)) state.Condition
                { state with Condition = cond; Versions = versions }
            | (HeapOp heapOp) ->
                let versions =
                    match HeapOperation.targetVariable heapOp with
                    | Some { Name = varName } ->
                        let curVersion = getVersion state.Versions varName
                        Map.add varName (curVersion + 1) state.Versions
                    | None ->
                        state.Versions
                let versionedHeapOp = addHeapOpVersions state.Versions heapOp
                let (heap, heapCond) = heapFn.PerformOp versionedHeapOp state.Heap
                let cond =
                    match heapCond with
                    | Some term -> condFn.Assert term state.Condition
                    | None -> state.Condition
                { state with Heap = heap; Condition = cond; Versions = versions }

        match edge with
        | Inner innerEdge ->
            let node = Graph.node graph innerEdge.From
            let path = Step (node, edge, state.Path)
            let edgeCondTerm = addVersions state.Versions innerEdge.Condition
            let state' = { state with Path = path; Condition = condFn.Assert edgeCondTerm state.Condition }
            match node with
            | Basic (_, operations) ->
                List.foldBack processOperation operations state'
            | _ ->
                state'
        | Outer _ ->
            failwith "Not implemented"

    let run condFn heapFn graph node =
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
                            |> List.map (Inner >> extend condFn heapFn graph state)
                            |> (fun addedStates -> List.append addedStates states')
                        step states'' results
                | _ ->
                    step states' results
        let states = [ { Path = Target node; Condition = condFn.GetEmpty(); Versions = Map.empty; Heap = heapFn.GetEmpty() } ]
        step states []
