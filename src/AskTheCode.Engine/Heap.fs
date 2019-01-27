namespace AskTheCode.Heap

open AskTheCode.Smt

type Class = Class of string

type ValueField = { ContainedIn: Class; Name: string; Sort: Sort }
type ReferenceField = { ContainedIn: Class; Name: string; Type: Class }

[<RequireQualifiedAccess>]
type Field =
    | Value of ValueField
    | Reference of ReferenceField

    member this.ContainedIn =
        match this with
        | Value { ContainedIn = klass } -> klass
        | Reference { ContainedIn = klass } -> klass
        
    member this.Name =
        match this with
        | Value { Name = name } -> name
        | Reference { Name = name } -> name


type Reference = { Type: Class; Name: string }

type HeapOperation =
    | New of Target: Reference * Type: Class
    | AssignEquals of Target: Variable * Left: Reference * Right: Reference
    | AssignNotEquals of Target: Variable * Left: Reference * Right: Reference
    | AssignRef of Target: Reference * Value: Reference
    | ReadRef of Target: Reference * Instance: Reference * Field: ReferenceField
    | WriteRef of Instance: Reference * Field: ReferenceField * Value: Reference
    | ReadVal of Target: Variable * Instance: Reference * Field: ValueField
    | WriteVal of Instance: Reference * Field: ValueField * Value: Term

module HeapOperation =
    let targetVariable heapOp =
        match heapOp with
        | AssignEquals (v, _, _)
        | AssignNotEquals (v, _, _)
        | ReadVal (v, _, _) ->
            Some v
        | _ ->
            None

type TypeSystem = { Classes: Class list; Fields: Field list }

module TypeSystem =

    let fields ts klass = List.filter (fun (field:Field) -> field.ContainedIn = klass) ts.Fields

    let ObjectType = Class "Object"

    let Null = { Type = ObjectType; Name = "null" }