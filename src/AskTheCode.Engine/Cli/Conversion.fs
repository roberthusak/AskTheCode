module AskTheCode.Cli.Conversion

open Microsoft.CodeAnalysis
open Microsoft.CodeAnalysis.Operations

open AskTheCode.Smt
open AskTheCode.Cfg
open AskTheCode.Cli.RoslynUtils
open AskTheCode.Heap

/// <summary>
/// Intermediate postfix form of operations, conceptually resembling CIL
/// </summary>
[<RequireQualifiedAccess>]
type Postfix =
    | LoadVar of Variable
    | LoadRef of Reference
    | Assign
    | LoadField of Field
    | StoreField of Field       // TODO: Consider using StoreVar as well
    | LoadInt of int
    | LoadBool of bool
    | Pop
    | Add
    | Neg
    | Lt
    | Leq
    | Gt
    | Geq
    | Eq
    | Neq
    | And
    | Or
    | Not

[<RequireQualifiedAccess>]
type Context =
    | Read
    | Write

type StackItem =
    | StackVal of Term
    | StackRef of Reference

let symbolToSort (typeSymbol:ITypeSymbol) =
        match typeSymbol.SpecialType with
        | SpecialType.System_Boolean -> Sort.Bool
        | SpecialType.System_Int32 -> Sort.Int
        | _ -> failwith "Not implemented"

let symbolToClass (typeSymbol:ITypeSymbol) =
    Class typeSymbol.Name

let symbolToField (fieldSymbol:IFieldSymbol) =
    let containedIn = symbolToClass fieldSymbol.ContainingType
    let name = fieldSymbol.Name
    match fieldSymbol.Type.IsReferenceType with
    | true ->
        let ``type`` = symbolToClass fieldSymbol.Type
        Field.Reference { ContainedIn = containedIn; Name = name; Type = ``type``}
    | false ->
        let sort = symbolToSort fieldSymbol.Type
        Field.Value { ContainedIn = containedIn; Name = name; Sort = sort }

let roslynToPostfix (op:IOperation) =

    let varToPostfix (``type``:ITypeSymbol) name =
        match ``type``.IsReferenceType with
        | true ->
            let klassType = symbolToClass ``type``
            Postfix.LoadRef { Type = klassType; Name = name }
        | false ->
            let sort = symbolToSort ``type``
            Postfix.LoadVar { Sort = sort; Name = name }

    let finishAssign target =
        match matchOperation target with
        | FieldReference fr ->
            Postfix.StoreField <| symbolToField fr.Field
        | _ ->
            Postfix.Assign

    let rec convert (ctx:Context) (op:IOperation) =
        seq {
            match matchOperation op with
            | Binary b ->
                yield! convert Context.Read b.LeftOperand
                yield! convert Context.Read b.RightOperand
                match b.OperatorKind with
                | BinaryOperatorKind.Add -> yield Postfix.Add
                | BinaryOperatorKind.LessThan -> yield Postfix.Lt
                | BinaryOperatorKind.LessThanOrEqual -> yield Postfix.Leq
                | BinaryOperatorKind.GreaterThan -> yield Postfix.Gt
                | BinaryOperatorKind.GreaterThanOrEqual -> yield Postfix.Geq
                | BinaryOperatorKind.Equals -> yield Postfix.Eq
                | BinaryOperatorKind.NotEquals -> yield Postfix.Neq
                | BinaryOperatorKind.And -> yield Postfix.And
                | BinaryOperatorKind.Or -> yield Postfix.Or
                | _ -> failwith "Not implemented"
            | Conversion c ->
                // TODO: Handle all the other cases
                assert (c.Type.SpecialType = SpecialType.System_Object)  // Conversion to Object before comparison with null
                yield! convert Context.Read c.Operand
            | ExpressionStatement es ->
                yield! convert Context.Read es.Operation
                yield Postfix.Pop
            | FieldReference fr ->
                yield! convert Context.Read fr.Instance
                match ctx with
                |  Context.Read ->
                    yield Postfix.LoadField <| symbolToField fr.Field
                | Context.Write ->
                    ()  // Done in finishAssign
            | InstanceReference ir ->
                match ir.ReferenceKind with
                | InstanceReferenceKind.ContainingTypeInstance ->
                    yield Postfix.LoadRef { Type = symbolToClass ir.Type; Name = "this" }
                | _ ->
                    failwith "Not implemented"
            | Literal l ->
                assert l.ConstantValue.HasValue
                match l.ConstantValue.Value with
                | :? int as i -> yield Postfix.LoadInt i
                | :? bool as b -> yield Postfix.LoadBool b
                | null -> yield Postfix.LoadRef TypeSystem.Null
                | _ -> failwith "Not implemented"
            | LocalReference lr ->
                yield varToPostfix lr.Type lr.Local.Name
            | ParameterReference pr ->
                yield varToPostfix pr.Type pr.Parameter.Name
            | SimpleAssignment sa ->
                yield! convert Context.Write sa.Target
                yield! convert Context.Read sa.Value
                yield finishAssign sa.Target
            | Unary u ->
                yield! convert Context.Read u.Operand
                match u.OperatorKind with
                | UnaryOperatorKind.Minus -> yield Postfix.Neg
                | _ -> failwith "Not implemented"
            | Unsupported ->
                failwith "Not implemented"
        }

    convert Context.Read op

// TODO: Refactor in a functional fasion (probably using VariableContext or sth. like that)
let mutable lastHlpVar = 0
let getFreeHlpVarName () =
    let no = lastHlpVar
    lastHlpVar <- lastHlpVar + 1
    sprintf "hlp%d" no

/// <summary>
/// Convert a sequence of <see cref="Postfix" /> operations to a list of CFG operations <see cref="Operation" />
/// and a possible final value.
/// </summary>
let postfixToOperations postOps =

    let convertBinary cons (stack, revOps) =
        match stack with
        | StackVal right :: StackVal left :: stack' ->
            let result = cons (left, right)
            (StackVal result :: stack', revOps)
        | _ ->
            failwith "Invalid operands"
            
    let convertUnary cons (stack, revOps) =
        match stack with
        | StackVal value :: stack' ->
            let result = cons value
            (StackVal result :: stack', revOps)
        | _ ->
            failwith "Invalid operand"

    let convertEquality cons heapCons (stack, revOps) =
        match stack with
        | StackRef right :: StackRef left :: stack' ->
            let hlpVar = { Sort = Bool; Name = getFreeHlpVarName() }
            (StackVal (Var hlpVar) :: stack', HeapOp (heapCons (hlpVar, left, right)) :: revOps)
        | _ ->
            convertBinary cons (stack, revOps)

    let convertPostOp (stack, revOps) postOp =
        match postOp with
        | Postfix.LoadVar v ->
            (StackVal (Var v) :: stack, revOps)
        | Postfix.LoadRef r ->
            (StackRef r :: stack, revOps)
        | Postfix.Assign ->
            match stack with
            | StackVal value :: StackVal (Var target) :: stack' ->
                (StackVal (Var target) :: stack', Assign { Target = target; Value = value} :: revOps)
            | StackRef value :: StackRef target :: stack' ->
                (StackRef target :: stack', HeapOp (AssignRef (target, value)) :: revOps)
            | _ ->
                failwith "Invalid assignment"
        | Postfix.LoadField field ->
            match stack with
            | StackRef instance :: stack' ->
                match field with
                | Field.Value valField ->
                    let hlpVar = { Sort = valField.Sort; Name = getFreeHlpVarName() }
                    (StackVal (Var hlpVar) :: stack', HeapOp (ReadVal (hlpVar, instance, valField)) :: revOps)
                | Field.Reference refField ->
                    let hlpRef = { Type = refField.Type; Name = getFreeHlpVarName() }
                    (StackRef hlpRef :: stack', HeapOp (ReadRef (hlpRef, instance, refField)) :: revOps)
            | _ ->
                failwith "Invalid field read"
        | Postfix.StoreField field ->
            match (field, stack) with
            | (Field.Value valField, StackVal value :: StackRef instance :: stack') ->
                (StackVal value :: stack', HeapOp (WriteVal (instance, valField, value)) :: revOps)
            | (Field.Reference refField, StackRef value :: StackRef instance :: stack') ->
                (StackRef value :: stack', HeapOp (WriteRef (instance, refField, value)) :: revOps)
            | _ ->
                failwith "Invalid field write"
        | Postfix.LoadInt i ->
            (StackVal (IntConst i) :: stack, revOps)
        | Postfix.LoadBool b ->
            (StackVal (BoolConst b) :: stack, revOps)
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
        | Postfix.Leq ->
            convertBinary Leq (stack, revOps)
        | Postfix.Gt ->
            convertBinary Gt (stack, revOps)
        | Postfix.Geq ->
            convertBinary Geq (stack, revOps)
        | Postfix.Eq ->
            convertEquality Eq AssignEquals (stack, revOps)
        | Postfix.Neq ->
            convertEquality Neq AssignNotEquals (stack, revOps)
        | Postfix.And ->
            convertBinary And (stack, revOps)
        | Postfix.Or ->
            convertBinary Or (stack, revOps)
        | Postfix.Not ->
            convertUnary Not (stack, revOps)

    let (stack, revOps) = Seq.fold convertPostOp ([], []) postOps
    let valOpt =
        match stack with
        | StackVal value :: _ -> Some value
        | _ -> None
    (List.rev revOps, valOpt)
