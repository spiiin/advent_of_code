open System
open System.Collections.Generic
open System.IO

type Direction = Up | Down | Left | Right

let turnRight direction =
    match direction with
    | Up -> Right
    | Right -> Down
    | Down -> Left
    | Left -> Up

let move (x, y) direction =
    match direction with
    | Up -> (x, y - 1)
    | Down -> (x, y + 1)
    | Left -> (x - 1, y)
    | Right -> (x + 1, y)

let parseMap (lines: string[]) =
    let mutable guardPosition = (0, 0)
    let mutable guardDirection = Up
    let obstacles = HashSet<(int * int)>()

    for y in 0 .. lines.Length - 1 do
        for x in 0 .. lines.[y].Length - 1 do
            match lines.[y].[x] with
            | '^' -> guardPosition <- (x, y)
            | '#' -> obstacles.Add((x, y)) |> ignore
            | _ -> ()
    (guardPosition, guardDirection, obstacles)

let simulateGuard (initialPosition, initialDirection, obstacles: #ISet<(int * int)>) (mapWidth, mapHeight) =
    let visited = HashSet<(int * int)>()
    let mutable position = initialPosition
    let mutable direction = initialDirection

    visited.Add(position) |> ignore

    let isInsideMap (x, y) = x >= 0 && x < mapWidth && y >= 0 && y < mapHeight
    let mutable continueMoving = true
    while continueMoving do
        let nextPosition = move position direction
        if not (isInsideMap (fst nextPosition, snd nextPosition)) || obstacles.Contains(nextPosition) then
            direction <- turnRight direction
        else
            position <- nextPosition
            visited.Add(position) |> ignore
    
        if not (isInsideMap (fst (move position direction), snd (move position direction))) then
            continueMoving <- false

    visited.Count

let simulateWithObstruction initialPosition initialDirection (obstacles: HashSet<(int * int)>) (mapWidth, mapHeight) obstruction =
    let visited = HashSet<(int * int * Direction)>()
    let mutable position = initialPosition
    let mutable direction = initialDirection

    obstacles.Add(obstruction) |> ignore

    let isInsideMap (x, y) = x >= 0 && x < mapWidth && y >= 0 && y < mapHeight

    let mutable isLoop = false
    let mutable continueMoving = true
    while continueMoving && not isLoop do
        let state = (fst position, snd position, direction)
        if visited.Contains(state) then
            isLoop <- true
        else
            visited.Add(state) |> ignore
            let nextPosition = move position direction
            if not (isInsideMap nextPosition) then
                continueMoving <- false
            elif obstacles.Contains(nextPosition) then
                direction <- turnRight direction
            else
                position <- nextPosition

    obstacles.Remove(obstruction) |> ignore
    isLoop

let findLoopObstructionPositions (initialPosition, initialDirection, obstacles: HashSet<(int * int)>) (mapWidth, mapHeight) =
    let potentialPositions =
        [for y in 0 .. mapHeight - 1 do
            for x in 0 .. mapWidth - 1 do
                let pos = (x, y)
                if pos <> initialPosition && not (obstacles.Contains(pos)) then
                    yield pos]

    potentialPositions
    |> List.filter (fun pos -> simulateWithObstruction initialPosition initialDirection obstacles (mapWidth, mapHeight) pos)
    |> List.length

[<EntryPoint>]
let main argv =
    let input = File.ReadAllLines("input-2024-6.txt")
    let mapData = parseMap input
    let mapSize = input.[0].Length, input.Length
    let result1 = simulateGuard mapData mapSize
    printfn "The guard will visit %d distinct positions." result1
    let result2 = findLoopObstructionPositions mapData mapSize
    printfn "There are %d positions where an obstruction causes a loop." result2
    0