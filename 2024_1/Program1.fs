open System.IO

let calculateTotalDistance leftList rightList =
    List.sum distances
    |> List.zip (List.sort leftList) (List.sort rightList)
    |> List.map (fun (l, r) -> abs (l - r))

let readInputFile filePath =
    let lines = File.ReadAllLines(filePath)
    let leftList, rightList = 
        lines
        |> Array.fold (fun (leftAcc, rightAcc) line ->
            let parts = line.Split([|' '; '\t'|]) |> Array.map int
            (parts.[0] :: leftAcc, parts.[1] :: rightAcc)
        ) ([], [])
    (List.rev leftList, List.rev rightList)

let inputFilePath = "input-2024-1.txt"
let outputFilePath = "output-2024-1.txt"

try
    let leftList, rightList = readInputFile inputFilePath
    let totalDistance = calculateTotalDistance leftList rightList
    File.WriteAllText(filePath, $"Total distance: {totalDistance}" )
with
| :? FileNotFoundException -> printfn $"{inputFilePath}"
| ex -> printfn $"Save error: {ex.Message}"
