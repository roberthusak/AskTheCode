module AskTheCode.Gui.Msagl

open System.Windows.Forms
open Microsoft.Msagl.GraphViewerGdi

open AskTheCode.Smt
open AskTheCode.Cfg

type MsaglGraph = Microsoft.Msagl.Drawing.Graph
type MsaglNode = Microsoft.Msagl.Drawing.Node
type MsaglEdge = Microsoft.Msagl.Drawing.Edge

let convertCfg cfg =
    let printNodeId (NodeId id) = sprintf "%d" id

    let graph = new MsaglGraph()
    let addNode (node:Node) =
        let msNode = graph.AddNode(printNodeId node.Id)
        msNode.LabelText <- Graph.printNode node
        ()
    let addEdge (edge:InnerEdge) =
        let label =
            match edge.Condition with
            | BoolConst true -> ""
            | _ -> Term.print edge.Condition
        graph.AddEdge(printNodeId edge.From, label, printNodeId edge.To) |> ignore
        ()
    List.iter addNode cfg.Nodes
    List.iter addEdge cfg.Edges
    graph

let displayGraph (graph:MsaglGraph) =
    let form = new Form()
    let gViewer = new GViewer();
    form.Controls.Add(gViewer)
    gViewer.Dock <- DockStyle.Fill
    gViewer.Graph <- graph

    form.ShowDialog() |> ignore
    ()
