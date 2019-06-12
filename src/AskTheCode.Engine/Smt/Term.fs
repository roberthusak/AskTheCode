namespace AskTheCode.Smt

open AskTheCode

type Sort =
    | Bool
    | Int
    | Array of Index: Sort * Value: Sort

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
    | Implies of Term * Term
    | Select of Term * Term
    | Store of Term * Term * Term

module Term =

    // Helper types for term pattern matching

    [<RequireQualifiedAccess>]
    type BinaryOp = Add | Lt | Leq | Gt | Geq | Eq | Neq | And | Or | Implies

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
            | Implies -> " ==> "
        member this.Sort =
            match this with
            | Add -> Int
            | Lt | Leq | Gt | Geq | Eq | Neq | And | Or | Implies -> Bool
        member this.Precedence = 
            match this with
            | Add -> 3
            | Lt | Leq | Gt | Geq | Eq | Neq -> 2
            | And | Or | Implies -> 1

    type UnaryOp with  
        member this.Symbol =
            match this with
            | Neg -> "-"
            | Not -> "!"
        member this.Sort =
            match this with
            | Neg -> Int
            | Not -> Bool

    let (|Variable|Constant|Unary|Binary|Function|) term =
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
        | Implies (a, b) -> Binary (BinaryOp.Implies, a, b)
        | Select (a, i) -> Function ("select", [ a; i])
        | Store (a, i, v) -> Function ("store", [ a; i; v ])
    
    // Helper functions for working with terms

    let rec sort term =
        match term with
        | Variable { Sort = sort; } ->
            sort
        | Constant (sort, _) ->
            sort
        | Unary (op, _) ->
            op.Sort
        | Binary (op, _, _) ->
            op.Sort
        | Function (name, _) ->
            match term with
            | Select (a, _) ->
                match sort a with
                | Array (Value = value) -> value
                | _ -> failwith "Non-array sort in read function"
            | Store (a, _, _) ->
                sort a
            | _ ->
                failwithf "Unknown function '%s'" name

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
        | Implies (a, b) -> Utils.lazyUpdateUnion2 Implies fn term (a, b)
        | Select (a, i) -> Utils.lazyUpdateUnion2 Select fn term (a, i)
        | Store (a, i, v) -> Utils.lazyUpdateUnion3 Store fn term (a, i, v)

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
        | Function (name, args) ->
            args
            |> List.map print
            |> String.concat ", "
            |> sprintf "%s(%s)" name
        | Variable v -> v.Name
        | Constant (_, value) -> value.ToString()

    let foldAnd left right =
        match (left, right) with
        | (BoolConst false, _)
        | (_, BoolConst false) ->
            BoolConst false
        | (BoolConst true, BoolConst true) ->
            BoolConst true
        | (BoolConst true, right) ->
            right
        | (left, BoolConst true) ->
            left
        | (left, right) ->
            And (left, right)

    let foldOr left right=
        match (left, right) with
        | (BoolConst true, _)
        | (_, BoolConst true) ->
            BoolConst true
        | (BoolConst false, BoolConst false) ->
            BoolConst false
        | (BoolConst false, right) ->
            right
        | (left, BoolConst false) ->
            left
        | (left, right) ->
            Or (left, right)

    let conjunction terms =
        if Seq.isEmpty terms then
            BoolConst true
        else
            Seq.fold foldAnd (BoolConst true) terms

    let disjunction terms =
        if Seq.isEmpty terms then
            BoolConst true
        else
            Seq.fold foldOr (BoolConst false) terms