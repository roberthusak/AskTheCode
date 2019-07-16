namespace AskTheCode.Smt

type Evaluation =
    | IntVal of int
    | BoolVal of bool

type Model = Term -> Evaluation

type SolveResult =
    | Sat of Model
    | Unsat
    | Unknown

module SolveResult =

    let isSat result =
        match result with
        | Sat _ -> true
        | _ -> false

    let isUnsat result =
        match result with
        | Unsat -> true
        | _ -> false

    let Unknown result =
        match result with
        | Unknown -> true
        | _ -> false