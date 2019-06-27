module AskTheCode.Z3

open Microsoft

open AskTheCode.Smt
open AskTheCode.SymbolicExecution

let mkContext () =
    new Z3.Context()

let rec sortToZ3 (ctx:Z3.Context) sort :Z3.Sort =
    match sort with
    | Sort.Bool -> ctx.BoolSort :> _
    | Sort.Int -> ctx.IntSort :> _
    | Sort.Array (index, value) -> ctx.MkArraySort(sortToZ3 ctx index, sortToZ3 ctx value) :> _

let rec termToZ3 (ctx:Z3.Context) term :Z3.Expr =
    let inner = termToZ3 ctx
    let innerArith innerTerm = inner innerTerm :?> Z3.ArithExpr
    let innerBool innerTerm = inner innerTerm :?> Z3.BoolExpr
    let innerArray innerTerm = inner innerTerm :?> Z3.ArrayExpr
    match term with
    | Var v -> ctx.MkConst(v.Name, sortToZ3 ctx v.Sort)
    | IntConst a -> ctx.MkInt(a) :> _
    | BoolConst a -> ctx.MkBool(a) :> _
    | Add (a, b) -> ctx.MkAdd(innerArith a, innerArith b) :> _
    | Neg a -> ctx.MkUnaryMinus(innerArith a) :> _
    | Lt (a, b) -> ctx.MkLt(innerArith a, innerArith b) :> _
    | Leq (a, b) -> ctx.MkLe(innerArith a, innerArith b) :> _
    | Gt (a, b) -> ctx.MkGt(innerArith a, innerArith b) :> _
    | Geq (a, b) -> ctx.MkGe(innerArith a, innerArith b) :> _
    | Eq (a, b) -> ctx.MkEq(inner a, inner b) :> _
    | Neq (a, b) -> ctx.MkDistinct(inner a, inner b) :> _
    | And (a, b) -> ctx.MkAnd(innerBool a, innerBool b) :> _
    | Or (a, b) -> ctx.MkOr(innerBool a, innerBool b) :> _
    | Not a -> ctx.MkNot(innerBool a) :> _
    | Implies (a, b) -> ctx.MkImplies(innerBool a, innerBool b) :> _
    | Select (a, i) -> ctx.MkSelect(innerArray a, inner i)
    | Store (a, i, v) -> ctx.MkStore(innerArray a, inner i, inner v) :> _

let rec termFromZ3 (expr:Z3.Expr) =
     match expr with
     | _ when expr.BoolValue <> Z3.Z3_lbool.Z3_L_UNDEF -> BoolVal (expr.BoolValue = Z3.Z3_lbool.Z3_L_TRUE)
     | _ when expr.IsIntNum -> IntVal (expr :?> Z3.IntNum).Int
     | _ -> failwith "Unknown Z3 value"

let runSolve ctx (solver:Z3.Solver) =
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

let solveZ3 (ctx:Z3.Context) (z3expr:Z3.BoolExpr) =
    use solver = ctx.MkSolver()
    solver.Assert(z3expr)
    runSolve ctx solver

let solve (ctx:Z3.Context) term =
    let z3expr = termToZ3 ctx term :?> Z3.BoolExpr
    solveZ3 ctx z3expr

type SolverState = { Solver: Z3.Solver; mutable Stack: Term list }

type ConditionTrace = { SolverState: SolverState; Stack: Term list }

let stackCondFn (ctx:Z3.Context) :Exploration.ConditionFunctions<ConditionTrace> =
    let baseSolver = ctx.MkSolver()
    let empty = { SolverState = { Solver = baseSolver; Stack = [] }; Stack = [] }
    let getEmpty () = empty

    let assertTerm term (trace:ConditionTrace) = { trace with Stack = term :: trace.Stack }

    let solve trace =
        let solver = trace.SolverState.Solver
        let rec reduceToCommon (solverStack, solverSize, traceStack, traceSize, pending) =
            match (solverStack, traceStack) with
            | (_, _) when Utils.refEq solverStack traceStack ->
                (solverStack, solverSize, traceStack, traceSize, pending)

            | (_, _) when solverSize > traceSize ->
                solver.Pop((uint32)(solverSize - traceSize))
                reduceToCommon (List.skip (solverSize - traceSize) solverStack, traceSize, traceStack, traceSize, pending)

            | (_, traceHead :: traceTail) when solverSize < traceSize ->
                reduceToCommon (solverStack, solverSize, traceTail, traceSize - 1, traceHead :: pending)

            | (solverHead :: solverTail, traceHead :: traceTail) when solverSize = traceSize ->
                solver.Pop()
                reduceToCommon (solverTail, solverSize - 1, traceTail, traceSize - 1, traceHead :: pending)

            | _ -> failwith "Unreachable"

        let (_, _, _, _, pending) =
            reduceToCommon (trace.SolverState.Stack, List.length trace.SolverState.Stack, trace.Stack, List.length trace.Stack, [])
        for term in pending do
            let z3expr = termToZ3 ctx term
            solver.Push()
            solver.Assert(z3expr :?> Z3.BoolExpr)
        trace.SolverState.Stack <- trace.Stack
        runSolve ctx solver

    {
        GetEmpty = getEmpty;
        Assert = assertTerm;
        Solve = solve;
    }

type MintermSet = Set<Z3.BoolExpr>

let wpFn (ctx:Z3.Context) :Exploration.WeakestPreconditionFn<MintermSet> =

    let empty =
        Set.singleton <| ctx.MkTrue()

    let assertTerm term minterms =
        let z3term = termToZ3 ctx term :?> Z3.BoolExpr
        Set.map (fun minterm -> ctx.MkAnd(minterm, z3term)) minterms

    let replace trg value minterms =
        let z3trg = termToZ3 ctx trg
        let z3val = termToZ3 ctx value
        Set.map (fun (minterm:Z3.BoolExpr) -> minterm.Substitute(z3trg, z3val) :?> Z3.BoolExpr) minterms
    
    let simplify minterms =
        Set.map (fun (minterm:Z3.BoolExpr) -> minterm.Simplify() :?> Z3.BoolExpr) minterms

    let solve (minterms:MintermSet) =
        solveZ3 ctx <| ctx.MkOr(minterms)

    {
        GetEmpty = (fun () -> empty);
        Assert = assertTerm;
        Replace = replace;
        Simplify = simplify;
        Merge = Set.unionMany;
        Solve = solve;
    }