open System.IO

let countXMAS (grid: string array) =
    let (rows, cols) = Array.length grid, grid.[0].Length
    let directions = [ (0, 1); (1, 0); (1, 1); (1, -1); (0, -1); (-1, 0); (-1, -1); (-1, 1) ]
    
    let isValid x y = x >= 0 && x < rows && y >= 0 && y < cols

    let findWord x y dx dy =
        [0..3]
        |> List.forall (fun i ->
            let nx, ny = x + i * dx, y + i * dy
            isValid nx ny && grid.[nx].[ny] = "XMAS".[i])

    let mutable count = 0
    for x in 0..rows-1 do
        for y in 0..cols-1 do
            for (dx, dy) in directions do
                if findWord x y dx dy then
                    count <- count + 1
    count

let count_MAS_in_X (grid: string array) =
    let checkMAS x1 y1 x2 y2 x3 y3 =
        (grid.[x1].[y1] = 'M' && grid.[x2].[y2] = 'A' && grid.[x3].[y3] = 'S') ||
        (grid.[x1].[y1] = 'S' && grid.[x2].[y2] = 'A' && grid.[x3].[y3] = 'M')

    let isXMASPattern cx cy =
        let diagonal1 = checkMAS (cx - 1) (cy - 1) cx cy (cx + 1) (cy + 1)
        let diagonal2 = checkMAS (cx - 1) (cy + 1) cx cy (cx + 1) (cy - 1)
        diagonal1 && diagonal2

    let mutable count = 0
    for cx in 1..grid.Length-2 do
        for cy in 1..grid.[0].Length-2 do
            if isXMASPattern cx cy then
                count <- count + 1
    count

let grid = File.ReadAllLines("input-2024-4.txt")
printfn $"Number of XMAS occurrences: {countXMAS grid}"
printfn $"Number of MAS in X occurrences: {count_MAS_in_X grid}"
