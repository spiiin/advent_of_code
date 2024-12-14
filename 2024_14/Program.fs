open System.IO

let readInputFromFile filePath =
    File.ReadAllLines(filePath)
    |> Array.toList

let parseInput input =
    input
    |> List.map (fun (line: string) ->
        let parts = line.Split([| 'p'; 'v'; '='; ','; ' ' |], System.StringSplitOptions.RemoveEmptyEntries)
        let px, py = int parts.[0], int parts.[1]
        let vx, vy = int parts.[2], int parts.[3]
        (px, py, vx, vy)
    )

let width, height = 101, 103
let filePath = "input-2024-14.txt"
let input = readInputFromFile filePath
let robots = parseInput input

let printGrid (grid: int array array) =
    for y in 0 .. height - 1 do
        let row =
            [0 .. width - 1]
            |> List.map (fun x -> 
                if grid.[x].[y] > 0 then string grid.[x].[y] else "."
            )
            |> String.concat ""
        printfn $"{row}" 
    printfn ""

let wrap value max =
    if value < 0 then value + max
    elif value >= max then value - max
    else value

let updatePosition (px, py, vx, vy) =
    let newPx = wrap (px + vx) width
    let newPy = wrap (py + vy) height
    (newPx, newPy, vx, vy)

let simulate robots steps =
    [1..steps]
    |> List.fold (fun state _ ->
        state |> List.map updatePosition
    ) robots

let trackPositions robots =
    let grid = Array.init width (fun _ -> Array.create height 0)
    robots |> List.iter (fun (px, py, _, _) ->
        grid.[px].[py] <- grid.[px].[py] + 1
    )
    grid

let simulateWithPrint robots steps =
    let mutable currentRobots = robots
    for step in 0 .. steps do
        printfn $"After {step} seconds:"
        let grid = trackPositions currentRobots
        printGrid grid
        currentRobots <- currentRobots |> List.map updatePosition
    currentRobots

let countQuadrants (grid: int array array) =
    let centerX, centerY = width / 2, height / 2
    let counts = Array.create 4 0

    for x in 0 .. width - 1 do
        for y in 0 .. height - 1 do
            if x <> centerX && y <> centerY && grid.[x].[y] > 0 then
                let quadrant =
                    if x < centerX && y < centerY then 0
                    elif x >= centerX && y < centerY then 1
                    elif x < centerX && y >= centerY then 2
                    else 3
                counts.[quadrant] <- counts.[quadrant] + grid.[x].[y]
    
    counts

let calculateSafetyFactor counts =
    counts |> Array.fold (*) 1

let robotsAfter100Seconds = simulateWithPrint robots 10000 //100 //for visual hacking >1.txt
let grid = trackPositions robotsAfter100Seconds
let quadrantCounts = countQuadrants grid
let safetyFactor = calculateSafetyFactor quadrantCounts

printfn $"Safety Factor: {safetyFactor}" 
