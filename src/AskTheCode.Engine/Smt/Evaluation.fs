namespace AskTheCode.Smt

type Evaluation =
    | IntVal of int
    | BoolVal of bool

type Model = Term -> Evaluation

type SolveResult =
    | Sat of Model
    | Unsat
    | Unknown
