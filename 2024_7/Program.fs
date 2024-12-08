open System.IO

let rec evaluateExpression numbers operators =
    match numbers, operators with
    | [x], [] -> x
    | x::y::restNums, op::restOps ->
        let result = 
            match op with
            | '+' -> x + y
            | '*' -> x * y
            | '|' -> int64 $"{x}{y}"
            | _ -> failwith "Unsupported operator"
        evaluateExpression (result :: restNums) restOps
    | _ -> failwith "Invalid input"

let rec generateOperators length operators =
    if length = 0 then [[]]
    else
        let smallerCombinations = generateOperators (length - 1) operators
        smallerCombinations |> List.collect (fun ops -> operators |> List.map (fun op -> op :: ops))

let canSolve targetValue numbers operators =
    if List.isEmpty numbers then false
    else
        let numOperators = List.length numbers - 1
        let operatorCombinations = generateOperators numOperators operators
        operatorCombinations
        |> List.exists (fun ops -> evaluateExpression numbers ops = targetValue)

let solveEquations equations =
    let operatorsPart1 = ['+'; '*']
    let operatorsPart2 = ['+'; '*'; '|']

    equations
    |> List.fold (fun (part1Total, part2Total) (target, numbers) ->
        let part1Valid = canSolve target numbers operatorsPart1
        let part2Valid = canSolve target numbers operatorsPart2
        let newPart1Total = if part1Valid then part1Total + target else part1Total
        let newPart2Total = if part2Valid then part2Total + target else part2Total
        (newPart1Total, newPart2Total)
    ) (0L, 0L)

let parseFile filePath =
    File.ReadAllLines filePath
    |> Array.map (fun line ->
        let parts = line.Split(':')
        if parts.Length <> 2 then failwith "Invalid line format"
        let target = int64 (parts.[0].Trim())
        let numbers = parts.[1].Trim().Split(' ') |> Array.map int64 |> Array.toList
        (target, numbers))
    |> Array.toList

let inputEquations = parseFile "input-2024-7.txt"

let part1Total, part2Total = solveEquations inputEquations
printfn $"Total calibration result (Part 1): {part1Total}" 
printfn $"Total calibration result (Part 2): {part2Total}" 