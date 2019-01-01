namespace AskTheCode.SymbolicExecution

open AskTheCode.Smt
open AskTheCode.Cfg

module Exploration =

    type State = { Path: Path; Condition: Term; Versions: Map<string, int> }

    let extend graph state (edge:Edge) =

        let getVersion versions name =
            Map.tryFind name versions |> Option.defaultValue 0

        let formatVersioned name version =
            sprintf "%s_%d" name version

        let rec addVersions versions term =
            match term with
            | Var v ->
                let version = getVersion versions v.Name
                Var { v with Name = formatVersioned v.Name version }
            | _ ->
                Term.updateChildren (addVersions versions) term

        let processAssignment (Assign assign) (cond, versions) =
            let trgName = assign.Target.Name
            let trgVersion = getVersion versions trgName
            let target = Var { assign.Target with Name = formatVersioned trgName trgVersion }
            let versions' = Map.add trgName (trgVersion + 1) versions
            let value = addVersions versions' assign.Value
            let cond' = And (cond, Eq (target, value))
            (cond', versions')

        match edge with
        | Inner innerEdge ->
            let edgeCondTerm = addVersions state.Versions innerEdge.Condition
            let node = Graph.node graph innerEdge.From
            let (cond, versions) =
                match node with
                | Basic (_, operations) ->
                    List.foldBack processAssignment operations (state.Condition, state.Versions)
                | _ ->
                    (state.Condition, state.Versions)
            let path = Step (node, edge, state.Path)
            { Path = path; Condition = cond; Versions = versions}
        | Outer _ ->
            failwith "Not implemented"

    let run solver graph node =
        let rec step states results =
            match states with
            | [] ->
                results
            | state :: states' ->
                let node = Path.node state.Path
                match solver state.Condition with
                | Sat _ ->
                    match node with
                    | Enter _ ->
                        step states' (state.Path :: results)
                    | _ ->
                        let states'' =
                            Graph.edgesTo graph node
                            |> List.map (Inner >> extend graph state)
                            |> List.append states'
                        step states'' results
                | _ ->
                    step states' results
        let states = [ { Path = Target node; Condition = BoolConst true; Versions = Map.empty } ]
        step states []
