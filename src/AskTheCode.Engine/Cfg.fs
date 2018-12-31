namespace AskTheCode.Cfg

open AskTheCode

type Assignment = { Target: Variable; Value: Term }

type Operation = Assign of Assignment

type NodeId = NodeId of int

type InnerNode = { Operations: Operation list }

type ReturnNode = { Value: Term }

type Node =
    | Inner of NodeId * InnerNode
    | Enter of NodeId
    | Return of NodeId * ReturnNode

    member this.Id =
        match this with
        | Inner (id, _) -> id
        | Enter id -> id
        | Return (id, _) -> id

type InnerEdge = { From: NodeId; To: NodeId; Condition: Term }

type Edge = InnerEdge

type Graph = { Nodes: Node list; Edges: Edge list }

module Graph =

    let node graph nodeId = List.find (fun (n:Node) -> n.Id = nodeId) graph.Nodes

    let edgesFrom graph (node:Node) = List.filter (fun e -> e.From = node.Id) graph.Edges

    let edgesTo graph (node:Node) = List.filter (fun e -> e.To = node.Id) graph.Edges