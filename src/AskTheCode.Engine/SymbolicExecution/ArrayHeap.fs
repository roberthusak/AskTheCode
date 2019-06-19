module AskTheCode.SymbolicExecution.ArrayHeap

open AskTheCode
open AskTheCode.Smt
open AskTheCode.Heap
open System

[<RequireQualifiedAccess>]
type EnvEval =
    | Val of Value: int
    | Var of Name: string

    member this.AsTerm =
        match this with
        | Val i -> Term.IntConst i
        | Var name -> Term.Var { Sort = Int; Name = name }

type Heap = { Environment: Map<Reference, EnvEval>; InitializedVars: Set<string>; FieldVersions: Map<Field, int>; ObjectCounter: int }

// Implementation

let nullEval = EnvEval.Val 0

let fieldSort = Sort.Array (Int, Int)

let empty = { Environment = Map.empty |> Map.add TypeSystem.Null nullEval; InitializedVars = Set.empty; FieldVersions = Map.empty; ObjectCounter = 0 }

// TODO: Either refactor in a functional fasion or move to Utils (e.g. 'getDistinctNumber()')
let mutable lastVer = 0
let getFreeVerNo () =
    let no = lastVer
    lastVer <- lastVer + 1
    no

let freshVarName refName =
    sprintf "%s!%d" refName <| getFreeVerNo()

let fieldVersion field versions =
    Map.tryFind field versions |> Option.defaultValue 0

let fieldVarName name version =
    sprintf "!%s!%d" name version

let getFieldVar (field:Field) version =
    Var { Sort = fieldSort; Name = fieldVarName field.Name version}

let mapFresh r env =
    let eval = EnvEval.Var <| freshVarName r.Name
    let env' = Map.add r eval env
    (env', eval)

let init r env =
    match Map.tryFind r env with
    | Some eval ->
        (env, eval)
    | None ->
        mapFresh r env

let performOp op heap =

    let performEqualOp cons trg ref1 ref2 =
        let (env, eval1) = init ref1 heap.Environment
        let (env', eval2) = init ref2 env
        ({ heap with Environment = env' }, Some <| Eq (Var trg, cons (eval1.AsTerm, eval2.AsTerm)))

    let performWriteOp ins field valTerm env =
        let (env, insEval) = init ins env
        let fieldVer = fieldVersion field heap.FieldVersions
        let fieldVerBefore = fieldVer + 1
        let fieldVersions = Map.add field fieldVerBefore heap.FieldVersions
        let fieldVar = Var { Sort = fieldSort; Name = fieldVarName field.Name fieldVer }
        let fieldVarBefore = Var { Sort = fieldSort; Name = fieldVarName field.Name fieldVerBefore }
        let cond = And (Neq (insEval.AsTerm, nullEval.AsTerm), Eq (fieldVar, Store (fieldVarBefore, insEval.AsTerm, valTerm)))
        ({ heap with Environment = env; FieldVersions = fieldVersions}, Some cond)

    match op with
    | AssignEquals (trg, ref1, ref2) ->
        performEqualOp Eq trg ref1 ref2
    | AssignNotEquals (trg, ref1, ref2) ->
        performEqualOp Neq trg ref1 ref2
    | AssignRef (trg, value) ->
        match (Map.tryFind trg heap.Environment, Map.tryFind value heap.Environment) with
        | (Some trgEval, Some valEval) ->
            let env = Map.remove trg heap.Environment
            ({ heap with Environment = env }, Some <| Eq (trgEval.AsTerm, valEval.AsTerm))
        | (Some trgEval, None) ->
            let env =
                heap.Environment
                |> Map.remove trg
                |> Map.add value trgEval
            ({ heap with Environment = env }, None)
        | (None, _) ->
            // We are not interested in the reference stored to trg
            (heap, None)
    | ReadRef (trg, ins, field) ->
        let (env, insEval) = init ins heap.Environment
        let cond = Neq (insEval.AsTerm, nullEval.AsTerm)
        match Map.tryFind trg heap.Environment with        // TODO: Use such optimization in ReadVal as well (if the variable is not read from anywhere)
        | Some trgEval ->
            match trgEval with
            | (EnvEval.Var trgVarName) ->
                let env = Map.remove trg env
                let inited = Set.add trgVarName heap.InitializedVars
                let fieldVar = Var { Sort = fieldSort; Name = fieldVarName field.Name <| fieldVersion (Field.Reference field) heap.FieldVersions }
                let cond = And (cond, Eq (trgEval.AsTerm, Select (fieldVar, insEval.AsTerm)))
                // TODO: In case of ReferenceField, constrain all the fields of trg to be <= 0 if trg < 0
                ({ heap with Environment = env; InitializedVars = inited }, Some cond)
            | _ ->
                failwithf "Invalid mapping of reference %A" trg
        | None ->
            // We are not interested in the returned reference, no need to reason about the dereference (apart from the instance not being null)
            ({ heap with Environment = env }, Some cond)
    | ReadVal (trg, ins, field) ->
        let (env, insEval) = init ins heap.Environment
        let fieldVar = Var { Sort = fieldSort; Name = fieldVarName field.Name <| fieldVersion (Field.Value field) heap.FieldVersions }
        let cond = And (Neq (insEval.AsTerm, nullEval.AsTerm), Eq (Var trg, Select (fieldVar, insEval.AsTerm)))
        ({ heap with Environment = env }, Some cond)
    | WriteRef (ins, field, value) ->
        let (env, valEval) = init value heap.Environment
        performWriteOp ins (Field.Reference field) valEval.AsTerm env
    | WriteVal (ins, field, value) ->
        performWriteOp ins (Field.Value field) value heap.Environment
    | New _ ->
        failwith "Not implemented"

let merge heaps =
    let mergedEnv = heaps |> Seq.map (fun heap -> heap.Environment) |> Seq.fold Utils.mergeMaps Map.empty
    let mergedInited = heaps |> Seq.map (fun heap -> heap.InitializedVars) |> Set.unionMany
    let mergedFieldVers = heaps |> Seq.map (fun heap -> heap.FieldVersions) |> Seq.fold Exploration.mergeVersions Map.empty
    let mergedObjCounter = heaps |> Seq.map (fun heap -> heap.ObjectCounter) |> Seq.max

    let fieldFolder cond field version =
        let mergedVersion = fieldVersion field mergedFieldVers
        if mergedVersion > version then
            Term.foldAnd cond <| Eq (getFieldVar field version, getFieldVar field mergedVersion)
        else
            cond

    let envFolder cond varRef eval =
        let mergedEval = Map.find varRef mergedEnv
        if eval <> mergedEval then
            Term.foldAnd cond <| Eq (eval.AsTerm, mergedEval.AsTerm)
        else
            cond

    let joinConds =
        seq {
            for heap in heaps do
                let joinCond = Map.fold fieldFolder (BoolConst true) heap.FieldVersions
                let joinCond = Map.fold envFolder joinCond heap.Environment
                yield joinCond
        }

    ({ Environment = mergedEnv; InitializedVars = mergedInited; FieldVersions = mergedFieldVers; ObjectCounter = mergedObjCounter }, joinConds)

let functions :Exploration.HeapFunctions<Heap> = { GetEmpty = (fun () -> empty); PerformOp = performOp; Merge = merge }