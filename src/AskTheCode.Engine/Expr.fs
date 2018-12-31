namespace Experiments

type Expr =
    | Var of string
    | Neg of Expr
    | And of Expr * Expr
    | Or of Expr * Expr

module Expr =

    let rec count expr =
        match expr with
         | Var a -> 1
         | Neg a -> count a + 1
         | And (a, b) -> (count a) + (count b) + 1
         | Or (a, b) -> (count a) + (count b) + 1

    let update cons fn orig a =
        let resA = fn a
        match LanguagePrimitives.PhysicalEquality a resA with
         | true -> orig
         | false -> cons resA

    let update2 cons fn orig a b =
        let resA = fn a
        let resB = fn b
        match LanguagePrimitives.PhysicalEquality a resA && LanguagePrimitives.PhysicalEquality b resB with
         | true -> orig
         | false -> cons (resA, resB)

    let rec traverse fn expr =
        match expr with
         | Var a -> expr
         | Neg a -> update Neg fn expr a
         | And (a, b) -> update2 And fn expr a b
         | Or (a, b) ->  update2 Or fn expr a b

    let rec nnf expr =
       match expr with
        | Neg (And (a, b)) -> Or (nnf (Neg a), nnf (Neg b))
        | Neg (Or (a, b)) -> And (nnf (Neg a), nnf (Neg b))
        | Neg (Neg a) -> nnf a
        | _ -> traverse nnf expr

    let rec cnf expr =
        let exprNnf = nnf expr
        match exprNnf with
         | Var a -> exprNnf
         | Neg a -> exprNnf
         | And (a, b) -> And (cnf a, cnf b)
         | Or (And (a, b), c) -> And (cnf <| Or (a, c), cnf <| Or (b, c))
         | Or (a, And (b, c)) -> And (cnf <| Or (a, b), cnf <| Or (a, c))
         | Or _ -> exprNnf

    let rec cnf2 expr =
        let exprNnf = nnf expr
        let rec processOr (a, b) =
            match (a, b) with
             | (And (a, b), c) -> And (processOr (a, c), processOr (b, c))
             | (a, And (b, c)) -> And (processOr (a, b), processOr (a, c))
             | (_, _) -> Or (a, b)
        match exprNnf with
         | Var a -> exprNnf
         | Neg a -> exprNnf
         | And (a, b) -> traverse cnf2 exprNnf
         | Or (a, b) -> processOr (a, b)