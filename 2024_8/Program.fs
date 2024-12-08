open System
open System.Collections.Generic
open System.IO

let findAntinodes map part2 =
    let height = List.length map
    let width = List.length (List.head map)
    let antennas = 
        [ for y in 0 .. height - 1 do
            for x in 0 .. width - 1 do
                if map.[y].[x] <> '.' then
                    yield (x, y, map.[y].[x]) ]
    let antinodes = HashSet<int * int>()
    let addAntinodesFromLine (x, y, dx, dy) =
        let mutable x, y = x + dx, y + dy
        while x >= 0 && x < width && y >= 0 && y < height do
            antinodes.Add((x, y)) |> ignore
            x <- x + dx
            y <- y + dy
    for i in 0 .. List.length antennas - 1 do
        for j in 0 .. List.length antennas - 1 do
            if i <> j then
                let (x1, y1, freq1) = antennas.[i]
                let (x2, y2, freq2) = antennas.[j]
                if freq1 = freq2 then
                    let dx, dy = x2 - x1, y2 - y1
                    if part2 then addAntinodesFromLine (x1, y1, dx, dy); addAntinodesFromLine (x2, y2, -dx, -dy)
                    else
                        let xAntinode1, yAntinode1 = x1 - dx, y1 - dy
                        let xAntinode2, yAntinode2 = x2 + dx, y2 + dy
                        if xAntinode1 >= 0 && xAntinode1 < width && yAntinode1 >= 0 && yAntinode1 < height then
                            antinodes.Add((xAntinode1, yAntinode1)) |> ignore
                        if xAntinode2 >= 0 && xAntinode2 < width && yAntinode2 >= 0 && yAntinode2 < height then
                            antinodes.Add((xAntinode2, yAntinode2)) |> ignore
    antinodes.Count

let filePath = "input-2024-8.txt"
let map = File.ReadLines(filePath) |> Seq.map (fun line -> line.ToCharArray() |> List.ofArray) |> Seq.toList
printfn $"Part 1: Total unique antinode locations: {findAntinodes map false}"
printfn $"Part 2: Total unique antinode locations: {findAntinodes map true}"