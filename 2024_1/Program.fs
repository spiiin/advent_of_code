open System
open System.IO

let calculateSimilarityScore leftList rightList =
    let rightCountMap = rightList |> Seq.countBy id |> Map.ofSeq
    leftList |> List.sumBy (fun number -> number * (Map.tryFind number rightCountMap |> Option.defaultValue 0))

let foldParsedInt (left, right) (line: string) =
    let parts = line.Split([|' '; '\t'|], StringSplitOptions.RemoveEmptyEntries) |> Array.map int
    (parts.[0] :: left, parts.[1] :: right)

let readInputFile filePath =
    File.ReadAllLines(filePath) 
    |> Array.fold foldParsedInt ([], [])

let leftList, rightList = readInputFile "input-2024-1.txt"
printfn $"{calculateSimilarityScore leftList rightList}"