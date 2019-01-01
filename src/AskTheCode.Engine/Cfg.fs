namespace AskTheCode.Cfg

open AskTheCode.Smt

type Assignment = { Target: Variable; Value: Term }

type Operation = Assign of Assignment

type NodeId = NodeId of int

type Node =
    | Basic of Id: NodeId * Operations: Operation list
    | Enter of Id: NodeId
    | Return of Id: NodeId * Value: Term option

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
