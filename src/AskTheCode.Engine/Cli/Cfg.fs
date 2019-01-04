module AskTheCode.Cli.Cfg

open Microsoft.CodeAnalysis.FlowAnalysis

open AskTheCode
open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.Cli.Conversion

let convertCfg (cfg:ControlFlowGraph) =

    // TODO: Handle creating multiple operations and nodes from one expression (e.g. when calling a routine)
    let convertBlock (block:BasicBlock) =
        let (nodes, valOpt) =
            let id = NodeId block.Ordinal
            match block.Kind with
            | BasicBlockKind.Block ->
                let (operations, valOpt) =
                    Seq.append block.Operations <| Utils.nullableToList block.BranchValue
                    |> Seq.map roslynToPostfix
                    |> Seq.concat
                    |> postfixToOperations
                let nodes =
                    match block.FallThroughSuccessor.Semantics with
                    | ControlFlowBranchSemantics.Regular ->
                        [ Basic (id, operations) ]
                    | ControlFlowBranchSemantics.Return ->
                        // TODO: Split to two nodes if there are any operations
                        if not block.Operations.IsEmpty then failwith "Not implemented" else ()
                        [ Return (id, valOpt) ]
                    | _ -> failwith "Not implemented"
                (nodes, valOpt)
            | BasicBlockKind.Entry ->
                ([ Enter id ], None)
            | BasicBlockKind.Exit ->
                ([], None)
            | _ ->
                failwith "Invalid enum value"
        let edges =
            match block.ConditionKind with
            | ControlFlowConditionKind.None ->
                if block.Kind <> BasicBlockKind.Exit && block.FallThroughSuccessor.Semantics <> ControlFlowBranchSemantics.Return
                    then [ { From = NodeId block.Ordinal; To = NodeId block.FallThroughSuccessor.Destination.Ordinal; Condition = BoolConst true } ]
                    else []
            | ControlFlowConditionKind.WhenTrue
            | ControlFlowConditionKind.WhenFalse ->
                let condTerm = Option.get valOpt        // It must contain a value left by block.BranchValue
                let fromId = NodeId block.Ordinal
                let conditionalId = NodeId block.ConditionalSuccessor.Destination.Ordinal
                let fallThroughId = NodeId block.FallThroughSuccessor.Destination.Ordinal
                let (conditionalCond, fallThroughCond) =
                    if block.ConditionKind = ControlFlowConditionKind.WhenTrue then (condTerm, Neg condTerm)
                    else (Neg condTerm, condTerm)
                [ { From = fromId; To = fallThroughId; Condition = fallThroughCond };
                  { From = fromId; To = conditionalId; Condition = conditionalCond } ]

            | _ ->
                failwith "Invalid enum value"
        (nodes, edges)

    let (nodes, edges) = Utils.accumulate2 convertBlock cfg.Blocks
    { Nodes = nodes; Edges = edges }
