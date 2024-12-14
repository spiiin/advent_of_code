open System
open System.Collections.Generic

let parseMap (lines: string[]) : char[][] =
    lines
    |> Array.map (fun line -> line.Trim().ToCharArray())

let directions = [| (0, 1); (1, 0); (0, -1); (-1, 0) |]

let renameGroups (grid: char[][]) : char[][] =
    let n, m = grid.Length, grid.[0].Length
    let visited = HashSet<int * int>()
    let mutable groupCounter = 0
    let renamedGrid = Array.init n (fun _ -> Array.create m ' ')

    let rec floodFill x y originalChar newChar =
        if x < 0 || y < 0 || x >= n || y >= m || visited.Contains((x, y)) || grid.[x].[y] <> originalChar then
            ()
        else
            visited.Add((x, y)) |> ignore
            renamedGrid.[x].[y] <- newChar
            for (dx, dy) in directions do
                floodFill (x + dx) (y + dy) originalChar newChar

    for i in 0 .. n - 1 do
        for j in 0 .. m - 1 do
            if not (visited.Contains((i, j))) then
                let originalChar = grid.[i].[j]
                let newChar = char (groupCounter + int 'A')
                floodFill i j originalChar newChar
                groupCounter <- groupCounter + 1

    renamedGrid

let floodFill (grid: char[][]) (x: int) (y: int) (visited: HashSet<int * int>) : (int * int) =
    let n = grid.Length
    let m = grid.[0].Length
    let regionType = grid.[x].[y]
    let queue = Queue<(int * int)>()
    queue.Enqueue((x, y))
    visited.Add((x, y)) |> ignore

    let mutable area = 0
    let mutable perimeter = 0

    while queue.Count > 0 do
        let (cx, cy) = queue.Dequeue()
        area <- area + 1

        for (dx, dy) in directions do
            let nx, ny = cx + dx, cy + dy
            if nx < 0 || ny < 0 || nx >= n || ny >= m then
                perimeter <- perimeter + 1
            else
                if grid.[nx].[ny] <> regionType then
                    perimeter <- perimeter + 1
                elif not (visited.Contains((nx, ny))) then
                    queue.Enqueue((nx, ny))
                    visited.Add((nx, ny)) |> ignore

    (area, perimeter)

let calculateTotalPrice (filePath: string) : int =
    let lines = System.IO.File.ReadAllLines(filePath)
    let originalGrid = parseMap lines
    let grid = renameGroups originalGrid
    let n = grid.Length
    let m = grid.[0].Length
    let visited = HashSet<int * int>()
    let mutable totalPrice = 0

    for i in 0 .. n - 1 do
        for j in 0 .. m - 1 do
            if not (visited.Contains((i, j))) then
                let area, perimeter = floodFill grid i j visited
                totalPrice <- totalPrice + (area * perimeter)

    totalPrice

let countCorners (grid: char[][]) =
    let rows, cols = grid.Length, grid.[0].Length
    let getValue x y =
        if x >= 0 && x < rows && y >= 0 && y < cols then Some grid.[x].[y] else None

    let updateCount counts symbol =
        match symbol with
        | Some s ->
            if Map.containsKey s counts then
                Map.add s (Map.find s counts + 1) counts
            else
                Map.add s 1 counts
        | None -> counts

    [ for x in -1 .. rows - 1 do
        for y in -1 .. cols - 1 ->
            let x11, x12, x21, x22 = getValue x y, getValue x (y + 1), getValue (x + 1) y, getValue (x + 1) (y + 1)
            if x11 = x12 && x11 = x21 && x11 = x22 then []
            else
                [ if x11 = x12 && x11 = x21 || x11 <> x12 && x11 <> x21 then x11 else None
                  if x12 = x11 && x12 = x22 || x12 <> x11 && x12 <> x22 then x12 else None
                  if x21 = x11 && x21 = x22 || x21 <> x11 && x21 <> x22 then x21 else None
                  if x22 = x12 && x22 = x21 || x22 <> x12 && x22 <> x21 then x22 else None ] ]
    |> List.concat
    |> List.fold updateCount Map.empty

let calculateTotalPriceDiscount (filePath: string) : int =
    let lines = System.IO.File.ReadAllLines(filePath)
    let originalGrid = parseMap lines
    let grid = renameGroups originalGrid
    let cornerCounts = countCorners grid

    cornerCounts
    |> Map.fold (fun acc symbol cornerCount ->
        let area =
            grid
            |> Array.mapi (fun x row ->
                row
                |> Array.mapi (fun y cell -> if cell = symbol then 1 else 0)
                |> Array.sum)
            |> Array.sum
        acc + (area * cornerCount)) 0

let filePath = "input-2024-12.txt"
printfn "Total price (perimeter): %d" (calculateTotalPrice filePath)
printfn "Total price (discount): %d" (calculateTotalPriceDiscount filePath)
Console.ReadKey() |> ignore
