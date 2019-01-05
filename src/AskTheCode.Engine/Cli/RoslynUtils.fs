namespace AskTheCode.Cli

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Operations

type RoslynOperation =
    | Binary of IBinaryOperation
    | Conversion of IConversionOperation
    | ExpressionStatement of IExpressionStatementOperation
    | FieldReference of IFieldReferenceOperation
    | InstanceReference of IInstanceReferenceOperation
    | Literal of ILiteralOperation
    | LocalReference of ILocalReferenceOperation
    | ParameterReference of IParameterReferenceOperation
    | SimpleAssignment of ISimpleAssignmentOperation
    | Unary of IUnaryOperation
    | Unsupported

module RoslynUtils =

    /// <summary>
    /// Casts the Roslyn IOperation to the appropriate specific interface according to its Kind
    /// </summary>
    let matchOperation (op:IOperation) =
        match op.Kind with
        | OperationKind.BinaryOperator -> Binary (op :?> _)
        | OperationKind.Conversion -> Conversion (op :?> _)
        | OperationKind.ExpressionStatement -> ExpressionStatement (op :?> _)
        | OperationKind.FieldReference -> FieldReference (op :?> _)
        | OperationKind.InstanceReference -> InstanceReference (op :?> _)
        | OperationKind.Literal -> Literal (op :?> _)
        | OperationKind.LocalReference -> LocalReference (op :?> _)
        | OperationKind.ParameterReference -> ParameterReference (op :?> _)
        | OperationKind.SimpleAssignment -> SimpleAssignment (op :?> _)
        | OperationKind.UnaryOperator -> Unary (op :?> _)
        | _ -> Unsupported
