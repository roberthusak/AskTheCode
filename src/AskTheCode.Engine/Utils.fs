module AskTheCode.Utils

let refEq = LanguagePrimitives.PhysicalEquality

let lazyUpdateUnion cons fn orig item =
    let res = fn item
    match refEq item res with
     | true -> orig
     | false -> cons res

let lazyUpdateUnion2 cons fn orig (item1, item2) =
    let res1 = fn item1
    let res2 = fn item2
    match refEq item1 res1 && refEq item2 res2 with
     | true -> orig
     | false -> cons (res1, res2)

let lazyUpdateUnion3 cons fn orig (item1, item2, item3) =
    let res1 = fn item1
    let res2 = fn item2
    let res3 = fn item3
    match refEq item1 res1 && refEq item2 res2 && refEq item3 res3 with
     | true -> orig
     | false -> cons (res1, res2, res3)

let accumulate fn inputs =
    let step results input =
        results @ fn input
    Seq.fold step [] inputs
    
let accumulate2 fn inputs =
    let step (results1, results2) input =
        let (next1, next2) = fn input
        (results1 @ next1, results2 @ next2)
    Seq.fold step ([], []) inputs

let nullableToList value =
    match value with
    | null -> []
    | _ -> [ value ]
