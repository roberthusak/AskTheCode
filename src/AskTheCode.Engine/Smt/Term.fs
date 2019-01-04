namespace AskTheCode.Smt

open AskTheCode

type Sort =
    | Bool
    | Int

type Variable = { Sort: Sort; Name: string }

type Term =
    | Var of Variable
    | IntConst of int
    | BoolConst of bool
    | Add of Term * Term
    | Neg of Term
    | Lt of Term * Term
    | Leq of Term * Term
    | Gt of Term * Term
    | Geq of Term * Term
    | Eq of Term * Term
    | Neq of Term * Term
    | And of Term * Term
    | Or of Term * Term
    | Not of Term

module Term =

    // Helper types for term pattern matching

    [<RequireQualifiedAccess>]
    type BinaryOp = Add | Lt | Leq | Gt | Geq | Eq | Neq | And | Or

    [<RequireQualifiedAccess>]
    type UnaryOp = Neg | Not

    type BinaryOp with  
        member this.Symbol =
            match this with
            | Add -> "+"
            | Lt -> "<"
            | Leq -> "<="
            | Gt -> ">"
            | Geq -> ">="
            | Eq -> "=="
            | Neq -> "!="
            | And -> " && "
            | Or -> " || "
        member this.Sort =
            match this with
            | Add -> Int
            | Lt | Leq | Gt | Geq | Eq | Neq | And | Or -> Bool
        member this.Precedence = 
            match this with
            | Add -> 3
            | Lt | Leq | Gt | Geq | Eq | Neq -> 2
            | And | Or -> 1

    type UnaryOp with  
        member this.Symbol =
            match this with
            | Neg -> "-"
            | Not -> "!"
        member this.Sort =
            match this with
            | Neg -> Int
            | Not -> Bool

    let (|Variable|Constant|Unary|Binary|) term =
        match term with
        | Var v -> Variable v
        | IntConst a -> Constant (Int, a :> System.Object)
        | BoolConst a -> Constant (Bool, a :> System.Object)
        | Add (a, b) -> Binary (BinaryOp.Add, a, b)
        | Neg a -> Unary (UnaryOp.Neg, a)
        | Lt (a, b) -> Binary (BinaryOp.Lt, a, b)
        | Leq (a, b) -> Binary (BinaryOp.Leq, a, b)
        | Gt (a, b) -> Binary (BinaryOp.Gt, a, b)
        | Geq (a, b) -> Binary (BinaryOp.Geq, a, b)
        | Eq (a, b) -> Binary (BinaryOp.Eq, a, b)
        | Neq (a, b) -> Binary (BinaryOp.Neq, a, b)
        | And (a, b) -> Binary (BinaryOp.And, a, b)
        | Or (a, b) -> Binary (BinaryOp.Or, a, b)
        | Not a -> Unary (UnaryOp.Not, a)
    
    // Helper functions for working with terms

    let sort term =
        match term with
        | Variable { Sort = sort; } -> sort
        | Constant (sort, _) -> sort
        | Unary (op, _) -> op.Sort
        | Binary (op, _, _) -> op.Sort

    let isLeaf term =
        match term with
        | Var _ | IntConst _ | BoolConst _ -> true
        | _ -> false

    let updateChildren fn term =
        match term with
        | Var _ | IntConst _ | BoolConst _ -> term
        | Add (a, b) -> Utils.lazyUpdateUnion2 Add fn term (a, b)
        | Neg a -> Utils.lazyUpdateUnion Neg fn term a
        | Lt (a, b) -> Utils.lazyUpdateUnion2 Lt fn term (a, b)
        | Leq (a, b) -> Utils.lazyUpdateUnion2 Leq fn term (a, b)
        | Gt (a, b) -> Utils.lazyUpdateUnion2 Gt fn term (a, b)
        | Geq (a, b) -> Utils.lazyUpdateUnion2 Geq fn term (a, b)
        | Eq (a, b) -> Utils.lazyUpdateUnion2 Eq fn term (a, b)
        | Neq (a, b) -> Utils.lazyUpdateUnion2 Neq fn term (a, b)
        | And (a, b) -> Utils.lazyUpdateUnion2 And fn term (a, b)
        | Or (a, b) -> Utils.lazyUpdateUnion2 Or fn term (a, b)
        | Not a -> Utils.lazyUpdateUnion Not fn term a

    let rec print expr =
        let parensPrint innerExpr = sprintf "(%s)" <| print innerExpr
        match expr with
        | Binary (op, left, right) ->
            let printInner innerExpr =
                match innerExpr with
                | Binary (innerOp, _, _) when innerOp.Precedence < op.Precedence -> parensPrint innerExpr
                | _ -> print innerExpr
            sprintf "%s %s %s" (printInner left) op.Symbol (printInner right)
        | Unary (op, operand) ->
            sprintf "%s%s" op.Symbol <| if isLeaf operand then print operand else parensPrint operand
        | Variable v -> v.Name
        | Constant (_, value) -> value.ToString()
