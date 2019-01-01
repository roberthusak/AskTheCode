namespace AskTheCode.Cli

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Operations

type RoslynOperation =
    | Binary of IBinaryOperation
    | ExpressionStatement of IExpressionStatementOperation
    | Literal of ILiteralOperation
    | ParameterReference of IParameterReferenceOperation
    | SimpleAssignment of ISimpleAssignmentOperation
    | Unary of IUnaryOperation
    | Unsupported

module RoslynUtils =

    // Casts the Roslyn IOperation to the appropriate specific interface according to its Kind
    let matchOperation (op:IOperation) =
        match op.Kind with
        | OperationKind.BinaryOperator -> Binary (op :?> _)
        | OperationKind.ExpressionStatement -> ExpressionStatement (op :?> _)
        | OperationKind.Literal -> Literal (op :?> _)
        | OperationKind.ParameterReference -> ParameterReference (op :?> _)
        | OperationKind.SimpleAssignment -> SimpleAssignment (op :?> _)
        | OperationKind.UnaryOperator -> Unary (op :?> _)
        | _ -> Unsupported
