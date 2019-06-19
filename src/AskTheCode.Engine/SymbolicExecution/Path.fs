namespace AskTheCode.SymbolicExecution

open AskTheCode.Cfg

type Path =
    | Target of Node
    | Step of Node * Edge * Path

module Path =

    let node path =
        match path with
        | Target node -> node
        | Step (node, _, _) -> node

    let rec print path =
        match path with
        | Target node ->
            let (NodeId id) = node.Id
            sprintf "[%d]" id
        | Step (node, _, path) ->
            let (NodeId id) = node.Id
            sprintf "[%d] ==> %s" id <| print path

    let rec nodes path =
        seq {
            match path with
            | Target node ->
                yield node
            | Step (node, _, tail) ->
                yield node
                yield! nodes tail
        }
