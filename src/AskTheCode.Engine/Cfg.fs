namespace AskTheCode.Cfg

open AskTheCode.Smt
open AskTheCode.Heap

type Assignment = { Target: Variable; Value: Term }

type Operation =
    | Assign of Assignment
    | HeapOp of HeapOperation

type NodeId = NodeId of int

module NodeId =
    let Value (NodeId id) = id

type NodeId with
    member this.Value = NodeId.Value this

type Node =
    | Basic of Id: NodeId * Operations: Operation list
    | Enter of Id: NodeId
    | Return of Id: NodeId * Value: Term option         // TODO: A reference may be returned as well

    member this.Id =
        match this with
        | Basic (id, _) -> id
        | Enter id -> id
        | Return (id, _) -> id

module Node =
    let Id (node:Node) = node.Id

    let withId node id =
        match node with
        | Basic (_, ops) -> Basic (id, ops)
        | Enter _ -> Enter id
        | Return (_, value) -> Return (id, value)

type InnerEdge = { From: NodeId; To: NodeId; Condition: Term }

type Graph = { Nodes: Node list; Edges: InnerEdge list }

type OuterEdge = { FromGraph: Graph; ToGraph: Graph; From: NodeId; To: NodeId; Condition: Term }

type Edge =
    | Inner of InnerEdge
    | Outer of OuterEdge

module Graph =
    open AskTheCode

    let node graph nodeId = List.find (fun (n:Node) -> n.Id = nodeId) graph.Nodes

    let edgesFromId graph id = List.filter (fun (e:InnerEdge) -> e.From = id) graph.Edges

    let edgesToId graph id = List.filter (fun (e:InnerEdge) -> e.To = id) graph.Edges

    let edgesFrom graph (node:Node) =  edgesFromId graph node.Id

    let edgesTo graph (node:Node) = edgesToId graph node.Id
    
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

    let dfs extender processor state graph (startNodeId:NodeId) =
        let nodeCount = List.length graph.Nodes
        let visited = Array.create nodeCount false
        let rec explore (stack, state) =
            match stack with
            | id :: stack ->
                match visited.[id] with
                | true ->
                    explore (stack, state)
                | false ->
                    visited.[id] <- true
                    let (state, cont) = processor state (NodeId id)
                    let extended =
                        match cont with
                        | true ->
                            extender (NodeId id)
                            |> List.map NodeId.Value
                        | false ->
                            []
                    explore (extended @ stack, state)
            | [] ->
                (stack, state)
        let (stack, state) = explore ([ startNodeId.Value ], state)
        assert (stack = [])
        state

    let forwardExtender graph id = edgesFromId graph id |> List.map (fun (e:InnerEdge) -> e.To)

    let backwardExtender graph id = edgesToId graph id |> List.map (fun (e:InnerEdge) -> e.From)

    type internal NodeColor = Unvisited = 0 | Visiting = 1 | Visited = 2

    let findLoops graph =
        let enterNode = List.find (fun n -> match n with | Enter _ -> true | _ -> false) graph.Nodes
        let nodeCount = List.length graph.Nodes
        let colors = Array.create nodeCount NodeColor.Unvisited

        let rec explore id (stack, results) =
            let recurse id (stack, results) =
                let folder (s, r) (edge:InnerEdge) = explore edge.To.Value (s, r)
                let (_, results) = List.fold folder (id :: stack, results) <| edgesFromId graph (NodeId id)
                (stack, results)
            let processResult id stack =
                let loopInside = List.takeWhile (fun i -> i <> id) stack
                id :: loopInside |> List.map NodeId

            match colors.[id] with
            | NodeColor.Visited ->
                (stack, results)
            | NodeColor.Unvisited ->
                colors.[id] <- NodeColor.Visiting
                let finalVal = recurse id (stack, results)
                colors.[id] <- NodeColor.Visited
                finalVal
            | NodeColor.Visiting ->
                (stack, processResult id stack :: results)
            | _ ->
                failwith "Invalid enum value"

        let (stack, results) = explore enterNode.Id.Value ([], [])
        assert (stack = [])
        results

    let unwindLoops graph count =
        assert (count > 0)

        let getStructure graph results (head, group) =
            let floodToHead state id =
                (Set.add id state, id <> head)
            let processLast state id =
                dfs (backwardExtender graph) floodToHead state graph id
            let bodyIds =
                List.map (List.tail >> List.head) group
                |> List.fold processLast Set.empty
            (head, bodyIds) :: results

        // Select the first loop which doesn't contain other loops
        let selectStructure structures =
            let notContainingOthers (head, structure) =
                List.forall (fun (h, _) -> h = head || not (Set.contains h structure)) structures
            let structure = List.find notContainingOthers structures
            (structure, List.except [ structure ] structures)

        let mapId idMap id =
            match Map.tryFind id idMap with
            | Some res -> res
            | None -> id
        let mapEdge idMapFrom idMapTo (edge:InnerEdge) =
            let fromId = mapId idMapFrom edge.From
            let toId = mapId idMapTo edge.To
            { edge with From = fromId; To = toId }

        let partitionEdges idsFrom idsTo edges =
            List.partition (fun (e:InnerEdge) -> Set.contains e.From idsFrom && Set.contains e.To idsTo) edges
            
        let filterEdges idsFrom idsTo edges =
            List.filter (fun (e:InnerEdge) -> Set.contains e.From idsFrom && Set.contains e.To idsTo) edges

        let adjacentEdges ids edges =
            List.filter (fun (e:InnerEdge) -> Set.contains e.From ids || Set.contains e.To ids) edges

        let rec unwindLoop graph structures =
            let ((headId, structure), structures) = selectStructure structures

            // Duplicate head and attach the end of the first (and only) iteration to it
            let origNodeCount = List.length graph.Nodes
            let forwardHead = Node.withId <| node graph headId <| NodeId origNodeCount
            let (backwardEdges, unaffectedEdges) = partitionEdges structure (Set.ofList [ headId ]) graph.Edges
            let headMap = (Map.ofList [(headId, forwardHead.Id)])
            let forwardEdges = List.map (mapEdge Map.empty headMap) backwardEdges
            let forwardHeadLeaveEdges =
                filterEdges (Set.ofList [ headId ]) (Set.difference (Set.ofList graph.Nodes |> Set.map Node.Id) structure) graph.Edges       // TODO: Consider turning Nodes into Set or ImmutableArray
                |> List.map (mapEdge headMap Map.empty)

            let nodes = forwardHead :: graph.Nodes
            let edges = forwardEdges @ forwardHeadLeaveEdges @ unaffectedEdges

            // Find edges to be duplicated in the loop
            let loopInnerAndLeaveEdges =
                adjacentEdges structure graph.Edges
                |> List.filter (fun (e:InnerEdge) -> e.To <> headId || Set.contains e.From structure)

            // Add count-1 copies of the loop to the graph
            let rec unwindStep (forwardHeadId, count, nodes, edges) =
                match count with
                | 0 ->
                    (nodes, edges, count)
                | _ ->
                    let allNodeCount = List.length nodes
                    let loopNodeCount = Set.count structure
                    let newIds = [allNodeCount .. allNodeCount + loopNodeCount - 1] |> List.map NodeId
                    let bodyMap =
                        Seq.fold2 (fun m k v -> Map.add k v m) Map.empty structure newIds
                    let newForwardHeadId = Map.find headId bodyMap
                    let duplNodes =
                        Set.map (fun id -> Node.withId (node graph id) (Map.find id bodyMap)) structure
                        |> List.ofSeq
                    let duplicateEdge (e:InnerEdge) =
                        if e.From = headId && not (Set.contains e.To structure)
                            then { e with From = newForwardHeadId }
                            else mapEdge (Map.add headId forwardHeadId bodyMap) bodyMap e
                    let duplEdges = List.map duplicateEdge loopInnerAndLeaveEdges
                    unwindStep (newForwardHeadId, count - 1, duplNodes @ nodes, duplEdges @ edges)
            let (nodes, edges, _) = unwindStep (forwardHead.Id, count - 1, nodes, edges)
            let graph = { Nodes = nodes; Edges = edges}

            match structures with
            | [] ->
                graph
            | _ ->
                // Update all the loops that contained this loop by adding the newly created node IDs and unwind the remaining loops
                let resultNodeCount = List.length nodes
                let createdNodes = [origNodeCount .. resultNodeCount - 1] |> List.map NodeId |> Set.ofList
                let updateStructure (hId, structure) =
                    match Set.contains headId structure with
                    | true ->
                        (hId, Set.union structure createdNodes)
                    | false ->
                        (hId, structure)
                let structures = List.map updateStructure structures
                unwindLoop graph structures

        match findLoops graph with
        | [] ->
            graph
        | loops ->
            List.groupBy List.head loops
            |> List.fold (getStructure graph) List.empty
            |> unwindLoop graph
