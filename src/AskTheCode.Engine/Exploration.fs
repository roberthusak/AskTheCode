namespace AskTheCode

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
        let edgeCondTerm = addVersions state.Versions edge.Condition
        let node = Graph.node graph edge.From
        let (cond, versions) =
            match node with
            | Inner (_, innerNode) ->
                List.foldBack processAssignment innerNode.Operations (state.Condition, state.Versions)
            | _ ->
                (state.Condition, state.Versions)
        let path = Step (node, edge, state.Path)
        { Path = path; Condition = cond; Versions = versions}

    let run solver graph node =
        let mutable states = [ { Path = Target node; Condition = BoolConst true; Versions = Map.empty } ]
        let mutable (results:Path list) = []
        let step states results : State list * Path list =
            let state = List.head states
            let states' = List.tail states
            let node = Path.node state.Path
            match solver state.Condition with
            | Sat _ ->
                match node with
                | Enter _ ->
                    (states', state.Path :: results)
                | _ ->
                    let states'' =
                        Graph.edgesTo graph node
                        |> List.map (extend graph state)
                        |> List.append states'
                    (states'', results)
            | _ ->
                (states', results)
        while not states.IsEmpty do
            let (newStates, newResults) = step states results
            states <- newStates
            results <- newResults
        results
