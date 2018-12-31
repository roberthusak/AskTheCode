module AskTheCode.Cfg.CSharp

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.CSharp
open Microsoft.CodeAnalysis.FlowAnalysis

open AskTheCode
open AskTheCode.Cfg
open Microsoft.CodeAnalysis.Operations

type RoslynOperation =
    | Binary of IBinaryOperation
    | ExpressionStatement of IExpressionStatementOperation
    | Literal of ILiteralOperation
    | ParameterReference of IParameterReferenceOperation
    | SimpleAssignment of ISimpleAssignmentOperation
    | Unary of IUnaryOperation
    | Unsupported

let matchOperation (op:IOperation) =
    match op.Kind with
    | OperationKind.BinaryOperator -> Binary (op :?> _)
    | OperationKind.ExpressionStatement -> ExpressionStatement (op :?> _)
    | OperationKind.Literal -> Literal (op :?> _)
    | OperationKind.ParameterReference -> ParameterReference (op :?> _)
    | OperationKind.SimpleAssignment -> SimpleAssignment (op :?> _)
    | OperationKind.UnaryOperator -> Unary (op :?> _)
    | _ -> Unsupported

let convertType (typeSymbol:ITypeSymbol) =
    match typeSymbol.SpecialType with
    | SpecialType.System_Boolean -> Sort.Bool
    | SpecialType.System_Int32 -> Sort.Int
    | _ -> failwith "Not implemented"

let rec convertExpression op =
    match matchOperation op with
    | Binary b ->
        match b.OperatorKind with
        | BinaryOperatorKind.GreaterThan -> Gt (convertExpression b.LeftOperand, convertExpression b.RightOperand)
        | BinaryOperatorKind.LessThan -> Lt (convertExpression b.LeftOperand, convertExpression b.RightOperand)
        | _ ->
            failwith "Not implemented"
    | Literal l ->
        assert l.ConstantValue.HasValue
        match l.ConstantValue.Value with
        | :? int as i -> IntConst i
        | :? bool as b -> BoolConst b
        | _ -> failwith "Not implemented"
    | ParameterReference r -> Var { Sort = convertType r.Type; Name = r.Parameter.Name }
    | Unary u ->
        match u.OperatorKind with
        | UnaryOperatorKind.Minus -> Neg (convertExpression u.Operand)
        | _ -> failwith "Not implemented"
    | _ ->
        failwith "Not implemented"

let rec convertOperation (op:IOperation) =
    match matchOperation op with
    | ExpressionStatement es ->
        convertOperation es.Operation
    | SimpleAssignment sa ->
        let targetVar =
            match matchOperation sa.Target with
            | ParameterReference r -> { Sort = convertType r.Type; Name = r.Parameter.Name }
            | _ -> failwith "Not implemented"
        let value = convertExpression sa.Value
        [ Assign { Target = targetVar; Value = value }]
    | _ ->
        failwith "Not implemented"

let convertCfg (cfg:ControlFlowGraph) =
    // TODO: Handle creating multiple operations and nodes from one expression (e.g. when calling a routine)
    let convertBlock (block:BasicBlock) =
        let id = NodeId block.Ordinal
        match block.Kind with
        | BasicBlockKind.Block ->
            // TODO: Try to get rid of the lambda
            let operations = Seq.fold (fun res op -> List.append res (convertOperation op)) [] block.Operations
            match block.FallThroughSuccessor.Semantics with
            | ControlFlowBranchSemantics.Regular ->
                [ Inner (id, { Operations = operations }) ]
            | ControlFlowBranchSemantics.Return ->
                // TODO: Split to two nodes if there are any operations
                if not block.Operations.IsEmpty then failwith "Not implemented" else ()
                let value = convertExpression block.BranchValue
                [ Return (id, { Value = value }) ]
            | _ -> failwith "Not implemented"
        | BasicBlockKind.Entry ->
            [ Enter id ]
        | BasicBlockKind.Exit ->
            []
        | _ ->
            failwith "Invalid enum value"
    // TODO: Append any operations from the condition to the operations of the previous node
    let convertEdges (block:BasicBlock) =
        match block.ConditionKind with
        | ControlFlowConditionKind.None ->
            if block.Kind <> BasicBlockKind.Exit && block.FallThroughSuccessor.Semantics <> ControlFlowBranchSemantics.Return
                then [ { From = NodeId block.Ordinal; To = NodeId block.FallThroughSuccessor.Destination.Ordinal; Condition = BoolConst true } ]
                else []
        | ControlFlowConditionKind.WhenFalse ->
            let condTerm = convertExpression block.BranchValue
            [ { From = NodeId block.Ordinal; To = NodeId block.FallThroughSuccessor.Destination.Ordinal; Condition = condTerm };
              { From = NodeId block.Ordinal; To = NodeId block.ConditionalSuccessor.Destination.Ordinal; Condition = Neg condTerm} ]
        | ControlFlowConditionKind.WhenTrue ->
            failwith "Not implemented"
        | _ ->
            failwith "Invalid enum value"
    let nodes =
        // TODO: Try to get rid of the lambda
        Seq.fold (fun res block -> List.append res (convertBlock block)) [] cfg.Blocks
    let edges =
        // TODO: Try to get rid of the lambda
        Seq.fold (fun res block -> List.append res (convertEdges block)) [] cfg.Blocks
    { Nodes = nodes; Edges = edges }