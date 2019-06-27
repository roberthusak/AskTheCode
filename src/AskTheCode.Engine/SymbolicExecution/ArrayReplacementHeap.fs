module AskTheCode.SymbolicExecution.ArrayReplacementHeap

open AskTheCode
open AskTheCode.Smt
open AskTheCode.Heap
open AskTheCode.SymbolicExecution.Exploration

// TODO: Either refactor in a functional fasion or move to Utils (e.g. 'getDistinctNumber()')
// TODO: Use in New when completed
let mutable lastObjectId = 0
let getFreeObjectId () =
    let no = lastObjectId
    lastObjectId <- lastObjectId + 1
    no

let getVarFromRef (reference:Reference) =
    Var { Name = "r!" + reference.Name; Sort = Int }

let getVarFromField (field:Field) =
    Var { Name = "f!" + field.Name; Sort = Array (Int, Int) }

let performOp op =
    match op with
    | AssignEquals (trg, ref1, ref2) ->
        Replace (Var trg, Eq (getVarFromRef ref1, getVarFromRef ref2))
    | AssignNotEquals (trg, ref1, ref2) ->
        Replace (Var trg, Neq (getVarFromRef ref1, getVarFromRef ref2))
    | AssignRef (trg, value) ->
        Replace (getVarFromRef trg, getVarFromRef value)
    | ReadRef (trg, ins, field) ->
        Replace (getVarFromRef trg, Select (getVarFromField <| Field.Reference field, getVarFromRef ins))
    | ReadVal (trg, ins, field) ->
        Replace (Var trg, Select (getVarFromField <| Field.Value field, getVarFromRef ins))
    | WriteRef (ins, field, value) ->
        let fieldVar = getVarFromField <| Field.Reference field
        Replace (fieldVar, Store (fieldVar, getVarFromRef ins, getVarFromRef value))
    | WriteVal (ins, field, value) ->
        let fieldVar = getVarFromField <| Field.Value field
        Replace (fieldVar, Store (fieldVar, getVarFromRef ins, value))
    | New _ ->
        failwith "Not implemented"

let merge heaps =
    ((), Seq.replicate (Seq.length heaps) <| Assert (BoolConst true))

let functions :Exploration.HeapFunctions<unit> =
    {
        GetEmpty = (fun () -> ());
        PerformOp = (fun op () -> ((), Some <| performOp op));
        Merge = merge
    }