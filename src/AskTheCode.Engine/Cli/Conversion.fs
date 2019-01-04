module AskTheCode.Cli.Conversion

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Operations

open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.Cli.RoslynUtils

/// <summary>
/// Intermediate postfix form of operations, conceptually resembling CIL
/// </summary>
[<RequireQualifiedAccess>]
type Postfix =
    | LoadVar of Variable
    | Assign
    | LoadInt of int
    | LoadBool of bool
    | Pop
    | Add
    | Neg
    | Lt
    | Gt
    | Eq
    | And
    | Or
    | Not

let typeToSort (typeSymbol:ITypeSymbol) =
        match typeSymbol.SpecialType with
        | SpecialType.System_Boolean -> Sort.Bool
        | SpecialType.System_Int32 -> Sort.Int
        | _ -> failwith "Not implemented"

let rec roslynToPostfix (op:IOperation) =
    seq {
        match matchOperation op with
        | Binary bin ->
            yield! roslynToPostfix bin.LeftOperand
            yield! roslynToPostfix bin.RightOperand
            match bin.OperatorKind with
            | BinaryOperatorKind.Add -> yield Postfix.Add
            | BinaryOperatorKind.LessThan -> yield Postfix.Lt
            | BinaryOperatorKind.GreaterThan -> yield Postfix.Gt
            | BinaryOperatorKind.Equals -> yield Postfix.Eq
            | BinaryOperatorKind.And -> yield Postfix.And
            | BinaryOperatorKind.Or -> yield Postfix.Or
            | _ -> failwith "Not implemented"
        | ExpressionStatement es ->
            yield! roslynToPostfix es.Operation
            yield Postfix.Pop
        | Literal l ->
            assert l.ConstantValue.HasValue
            match l.ConstantValue.Value with
            | :? int as i -> yield Postfix.LoadInt i
            | :? bool as b -> yield Postfix.LoadBool b
            | _ -> failwith "Not implemented"
        | ParameterReference r ->
            yield Postfix.LoadVar { Sort = typeToSort r.Type; Name = r.Parameter.Name }
        | SimpleAssignment a ->
            yield! roslynToPostfix a.Target
            yield! roslynToPostfix a.Value
            yield Postfix.Assign
        | Unary u ->
            yield! roslynToPostfix u.Operand
            match u.OperatorKind with
            | UnaryOperatorKind.Minus -> yield Postfix.Neg
            | _ -> failwith "Not implemented"
        | Unsupported ->
            failwith "Not implemented"
    }

/// <summary>
/// Convert a sequence of <see cref="Postfix" /> operations to a list of CFG operations <see cref="Operation" />
/// and a possible final value.
/// </summary>
let postfixToOperations postOps =

    let convertBinary cons (stack, revOps) =
        match stack with
        | right :: left :: stack' ->
            let result = cons (left, right)
            (result :: stack', revOps)
        | _ ->
            failwith "Invalid operands"
            
    let convertUnary cons (stack, revOps) =
        match stack with
        | value :: stack' ->
            let result = cons value
            (result :: stack', revOps)
        | _ ->
            failwith "Invalid operand"

    let convertPostOp (stack, revOps) postOp =
        match postOp with
        | Postfix.LoadVar v ->
            (Var v :: stack, revOps)
        | Postfix.Assign ->
            match stack with
            | value :: Var var :: stack' ->
                (Var var :: stack', Assign { Target = var; Value = value} :: revOps)
            | _ ->
                failwith "Invalid assignment"
        | Postfix.LoadInt i ->
            (IntConst i :: stack, revOps)
        | Postfix.LoadBool b ->
            (BoolConst b :: stack, revOps)
        | Postfix.Pop ->
            match stack with
            | (_ :: stack') ->
                (stack', revOps)
            | _ ->
                failwith "No item on stack to pop"
        | Postfix.Add ->
            convertBinary Add (stack, revOps)
        | Postfix.Neg ->
            convertUnary Neg (stack, revOps)
        | Postfix.Lt ->
            convertBinary Lt (stack, revOps)
        | Postfix.Gt ->
            convertBinary Gt (stack, revOps)
        | Postfix.Eq ->
            convertBinary Eq (stack, revOps)
        | Postfix.And ->
            convertBinary And (stack, revOps)
        | Postfix.Or ->
            convertBinary Or (stack, revOps)
        | Postfix.Not ->
            convertUnary Not (stack, revOps)

    let (stack, revOps) = Seq.fold convertPostOp ([], []) postOps
    let valOpt =
        match stack with
        | [ value ] -> Some value
        | [] -> None
        | _ -> failwith "More than one value on the stack after the operation conversion"
    (List.rev revOps, valOpt)
