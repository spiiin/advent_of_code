type Operator =
    | Add
    | Multiply
    | Concatenate

let apply a b operator =
    match operator with
    | Add -> a + b
    | Multiply -> a * b
    | Concatenate -> int64 ($"{a}{b}")

let canSolveWith operators (result, numbers) =
    let rec canSolve stack =
        match stack with
        | [] -> false
        | numbers :: stack ->
            match numbers with
            | [result'] when result' = result -> true
            | [ _ ] -> canSolve stack
            | a :: b :: numbers ->
                let newStacks =
                    operators
                    |> List.fold (fun acc operator ->
                        let value = apply a b operator
                        if value > result then acc else (value :: numbers) :: acc) stack
                canSolve newStacks
            | _ -> failwith "Invalid stack"
    canSolve [ numbers ]

let parseEquation (line: string) =
    match line.Split(": ") with
    | [| result; numbers |] ->
        (int64 result, numbers.Split(" ") |> Array.map int64 |> List.ofArray)
    | _ -> failwith "Invalid equation format"

let solve equations operators =
    equations
    |> List.sumBy (fun equation ->
        if canSolveWith operators equation then fst equation else 0L)

let input = System.IO.File.ReadAllLines("input-2024-7.txt")
let equations = input |> Array.map parseEquation |> Array.toList

let part1 = solve equations [ Add; Multiply ]
let part2 = solve equations [ Add; Multiply; Concatenate ]

printfn $"Part 1 Total: {part1}" 
printfn $"Part 2 Total: {part2}"