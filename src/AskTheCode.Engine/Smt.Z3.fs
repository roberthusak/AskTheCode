module AskTheCode.Z3

open Microsoft

open AskTheCode.Smt

let mkContext () =
    new Z3.Context()

let sortToZ3 (ctx:Z3.Context) sort :Z3.Sort =
    match sort with
    | Sort.Bool -> ctx.BoolSort :> _
    | Sort.Int -> ctx.IntSort :> _

let rec termToZ3 (ctx:Z3.Context) term :Z3.Expr =
    let inner = termToZ3 ctx
    let innerArith innerTerm = inner innerTerm :?> Z3.ArithExpr
    let innerBool innerTerm = inner innerTerm :?> Z3.BoolExpr
    match term with
    | Var v -> ctx.MkConst(v.Name, sortToZ3 ctx v.Sort)
    | IntConst a -> ctx.MkInt(a) :> _
    | BoolConst a -> ctx.MkBool(a) :> _
    | Add (a, b) -> ctx.MkAdd(innerArith a, innerArith b) :> _
    | Neg a -> ctx.MkUnaryMinus(innerArith a) :> _
    | Lt (a, b) -> ctx.MkLt(innerArith a, innerArith b) :> _
    | Gt (a, b) -> ctx.MkGt(innerArith a, innerArith b) :> _
    | Eq (a, b) -> ctx.MkEq(inner a, inner b) :> _
    | And (a, b) -> ctx.MkAnd(innerBool a, innerBool b) :> _
    | Or (a, b) -> ctx.MkOr(innerBool a, innerBool b) :> _
    | Not a -> ctx.MkNot(innerBool a) :> _

let rec termFromZ3 (expr:Z3.Expr) =
     match expr with
     | _ when expr.BoolValue <> Z3.Z3_lbool.Z3_L_UNDEF -> BoolVal (expr.BoolValue = Z3.Z3_lbool.Z3_L_TRUE)
     | _ when expr.IsIntNum -> IntVal (expr :?> Z3.IntNum).Int
     | _ -> failwith "Unknown Z3 value"

let solve (ctx:Z3.Context) term =
    let z3expr = termToZ3 ctx term
    use solver = ctx.MkSolver()
    solver.Assert(z3expr :?> Z3.BoolExpr)
    let z3status = solver.Check()
    match z3status with
    | Z3.Status.UNSATISFIABLE -> Unsat
    | Z3.Status.UNKNOWN -> Unknown
    | Z3.Status.SATISFIABLE ->
        let z3model = solver.Model
        let model modelledTerm =
            let modelledZ3Expr = termToZ3 ctx modelledTerm
            termFromZ3 <| z3model.Eval(modelledZ3Expr, true)
        Sat model
    | _ -> failwith "Invalid return type from Z3"
