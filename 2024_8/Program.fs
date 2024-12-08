open System
open System.IO
open System.Collections.Generic

type Position = int * int

let parseInput (lines: string seq) =
    lines
    |> Seq.mapi (fun y row -> row |> Seq.mapi (fun x c -> c, (x, y)))
    |> Seq.collect id
    |> Seq.filter (fst >> ((<>) '.'))
    |> Seq.groupBy fst
    |> Seq.map (fun (freq, positions) -> freq, positions |> Seq.map snd |> Seq.toList)
    |> Seq.toList

let isInsideMap (width, height) (x, y) =
    x >= 0 && x < width && y >= 0 && y < height

let findAntinodesPure (width, height) (x1, y1) (x2, y2) =
    let dx, dy = x2 - x1, y2 - y1
    [ (x1 - dx, y1 - dy); (x2 + dx, y2 + dy) ]
    |> List.filter (isInsideMap (width, height))

let findAntinodesHarmonic (width, height) (x1, y1) (x2, y2) =
    let dx, dy = x2 - x1, y2 - y1
    let rec generateAntinodes x y dx dy acc =
        if isInsideMap (width, height) (x, y) then
            generateAntinodes (x + dx) (y + dy) dx dy ((x, y) :: acc)
        else acc
    let forward = generateAntinodes x1 y1 dx dy []
    let backward = generateAntinodes x2 y2 -dx -dy []
    forward @ backward

let countAntinodes lines part2 =
    let antennas = parseInput lines
    let width, height = String.length (Seq.head lines), Seq.length lines
    let antinodeSet = HashSet<Position>()
    for _, positions in antennas do
        for i = 0 to List.length positions - 1 do
            for j = i + 1 to List.length positions - 1 do
                let (x1, y1), (x2, y2) = positions.[i], positions.[j]
                let antinodes =
                    if part2 then findAntinodesHarmonic (width, height) (x1, y1) (x2, y2)
                    else findAntinodesPure (width, height) (x1, y1) (x2, y2)
                for antinode in antinodes do
                    antinodeSet.Add antinode |> ignore
    antinodeSet.Count

[<EntryPoint>]
let main _ =
    let lines = File.ReadAllLines("input-2024-8.txt")
    printfn $"Part 1: {countAntinodes lines false}"
    printfn $"Part 2: {countAntinodes lines true}"
    0
