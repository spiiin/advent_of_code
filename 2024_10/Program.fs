open System
open System.IO
open System.Collections.Generic

let readMapFromFile (filename: string) =
    File.ReadAllLines(filename)
    |> Array.map (fun line -> line.ToCharArray() |> Array.map (fun c -> int (string c)))

let map = readMapFromFile "input-2024-10.txt"

let rows = map.Length
let cols = map.[0].Length

let neighbors (x, y) =
    [ (x - 1, y); (x + 1, y); (x, y - 1); (x, y + 1) ]
    |> List.filter (fun (nx, ny) -> nx >= 0 && ny >= 0 && nx < rows && ny < cols)

let rec findTrails (x, y) visited =
    let currentHeight = map.[x].[y]
    if currentHeight = 9 then
        Set.singleton (x, y)
    else
        neighbors (x, y)
        |> List.filter (fun (nx, ny) -> 
            not (Set.contains (nx, ny) visited) && map.[nx].[ny] = currentHeight + 1)
        |> List.fold (fun acc (nx, ny) -> 
            acc + (findTrails (nx, ny) (Set.add (nx, ny) visited))
        ) Set.empty

let routeCache = Dictionary<(int * int), int>()

let rec countUniqueTrails (x, y) =
    if routeCache.ContainsKey((x, y)) then
        routeCache.[(x, y)]
    else
        let currentHeight = map.[x].[y]
        if currentHeight = 9 then
            1
        else
            let totalRoutes =
                neighbors (x, y)
                |> List.filter (fun (nx, ny) -> map.[nx].[ny] = currentHeight + 1)
                |> List.sumBy (fun (nx, ny) -> countUniqueTrails (nx, ny))
            routeCache.[(x, y)] <- totalRoutes
            totalRoutes

let findTrailheads (map: int array array) =
    seq {
        for x in 0 .. rows - 1 do
            for y in 0 .. cols - 1 do
                if map.[x].[y] = 0 then yield (x, y)
    } |> Seq.toList

let calculateScores map =
    let trailheads = findTrailheads map
    trailheads
    |> List.map (fun (x, y) -> findTrails (x, y) (Set.singleton (x, y)) |> Set.count)
    |> List.sum

let calculateRatings map =
    let trailheads = findTrailheads map
    trailheads
    |> List.map (fun (x, y) -> countUniqueTrails (x, y))
    |> List.sum

let totalScore = calculateScores map
let totalRating = calculateRatings map

printfn "Sum of trailhead scores (Part One): %d" totalScore
printfn "Sum of trailhead ratings (Part Two): %d" totalRating
