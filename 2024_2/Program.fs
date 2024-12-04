open System
open System.IO

let isSafe levels =
    let differences = 
        levels |> List.pairwise |> List.map (fun (a, b) -> b - a)
    let isIncreasing = differences |> List.forall (fun diff -> diff > 0 && diff <= 3)
    let isDecreasing = differences |> List.forall (fun diff -> diff < 0 && diff >= -3)
    isIncreasing || isDecreasing

//explore some optimization
let findProblematicIndicesWithNeighbors levels =
    let n = List.length levels
    let problematic =
        [0 .. n - 2]
        |> List.choose (fun i ->
            let diff = levels.[i + 1] - levels.[i]
            if abs diff > 3 || (i > 0 && (levels.[i + 1] - levels.[i]) * (levels.[i] - levels.[i - 1]) <= 0)
            then Some i
            else None
        )
    let neighbors =
        problematic
        |> List.collect (fun i -> [i - 1; i; i + 1])
        |> List.filter (fun i -> i >= 0 && i < n)
    List.distinct neighbors //[0..levels.Length] //uncomment to re-check every element removal

let isSafeWithDampener levels =
    findProblematicIndicesWithNeighbors levels
    |> List.exists (fun index ->
        let filtered = List.mapi (fun i x -> if i <> index then Some x else None) levels |> List.choose id
        isSafe filtered
    )

let isSafeReport levels =
    isSafe levels || isSafeWithDampener levels

let readReports filePath =
    File.ReadAllLines(filePath)
    |> Array.map (fun line -> 
        line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
        |> Array.map int
        |> Array.toList
    )
    |> Array.toList

let countSafeReports reports =
    reports |> List.filter isSafeReport|> List.length

let reports = readReports "input-2024-2.txt"
printfn $"{countSafeReports reports}"