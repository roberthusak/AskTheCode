namespace AskTheCode.Cfg

open AskTheCode.Smt
open AskTheCode.Heap

type Assignment = { Target: Variable; Value: Term }

type Operation =
    | Assign of Assignment
    | HeapOp of HeapOperation

type NodeId = NodeId of int

type Node =
    | Basic of Id: NodeId * Operations: Operation list
    | Enter of Id: NodeId
    | Return of Id: NodeId * Value: Term option         // TODO: A reference may be returned as well

    member this.Id =
        match this with
        | Basic (id, _) -> id
        | Enter id -> id
        | Return (id, _) -> id

type InnerEdge = { From: NodeId; To: NodeId; Condition: Term }

type Graph = { Nodes: Node list; Edges: InnerEdge list }

type OuterEdge = { FromGraph: Graph; ToGraph: Graph; From: NodeId; To: NodeId; Condition: Term }

type Edge =
    | Inner of InnerEdge
    | Outer of OuterEdge

module Graph =

    let node graph nodeId = List.find (fun (n:Node) -> n.Id = nodeId) graph.Nodes

    let edgesFrom graph (node:Node) = List.filter (fun (e:InnerEdge) -> e.From = node.Id) graph.Edges

    let edgesTo graph (node:Node) = List.filter (fun (e:InnerEdge) -> e.To = node.Id) graph.Edges
    
    let printOperation op =
        match op with
        | Assign { Target = trg; Value = value } ->
            sprintf "%s <- %s" trg.Name <| Term.print value
        | HeapOp heapOp ->
            match heapOp with
            | New (trg, Class className) ->
                sprintf "%s <- new %s" trg.Name className
            | AssignEquals (trg, left, right) ->
                sprintf "%s <- %s == %s" trg.Name left.Name right.Name
            | AssignNotEquals (trg, left, right) ->
                sprintf "%s <- %s != %s" trg.Name left.Name right.Name
            | AssignRef (trg, value) ->
                sprintf "%s <- %s" trg.Name value.Name
            | ReadRef (trg, ins, field) ->
                sprintf "%s <- %s.%s" trg.Name ins.Name field.Name
            | WriteRef (ins, field, value) ->
                sprintf "%s.%s <- %s" ins.Name field.Name value.Name
            | ReadVal (trg, ins, field) ->
                sprintf "%s <- %s.%s" trg.Name ins.Name field.Name
            | WriteVal (ins, field, value) ->
                sprintf "%s.%s <- %s" ins.Name field.Name <| Term.print value

    let printNode (node:Node) =
        let (NodeId id) = node.Id
        let res = sprintf "[%i]" id
        match node with
        | Basic (_, ops) ->
            match ops with
            | [] ->
                res
            | _ ->
                ops
                |> List.map printOperation
                |> String.concat "\n"
                |> sprintf "%s\n%s\n" res
        | Enter _ ->
            res
        | Return (_, value) ->
            value
            |> Option.map Term.print
            |> Option.fold (+) ""
            |> sprintf "%s\nreturn %s" res
    
    let printEdge (edge:InnerEdge) =
        let (NodeId trgId) = edge.To
        match edge.Condition with
        | BoolConst true ->
            sprintf "goto [%i]" trgId
        | _ ->
            sprintf "if (%s) goto [%i]" (Term.print edge.Condition) trgId

    let print graph =
        let printNodeWithEdges node =
            edgesFrom graph node
            |> List.map printEdge
            |> String.concat "\n"
            |> sprintf "%s\n%s" (printNode node)
        graph.Nodes
        |> List.map printNodeWithEdges
        |> String.concat "\n\n"
