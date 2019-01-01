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
