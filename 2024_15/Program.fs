open System
open System.IO

let readInput (filePath: string) =
    let lines = File.ReadAllLines(filePath)
    let blankLineIndex = Array.findIndex (fun line -> line = "") lines
    let map = lines.[0..blankLineIndex - 1] |> Array.map (fun line -> line.ToCharArray())
    let moves = lines.[blankLineIndex + 1..] |> String.concat "" |> Seq.toArray
    map, moves

let moveRobot (map: char[][]) moves =
    let directions = dict ['^', (-1, 0); 'v', (1, 0); '<', (0, -1); '>', (0, 1)]
    let height, width = map.Length, map.[0].Length

    let findRobot () =
        let mutable position = (-1, -1)
        for i in 0 .. height - 1 do
            for j in 0 .. width - 1 do
                if map.[i].[j] = '@' then position <- (i, j)
        position

    let canMoveChain (row: int) (col: int) (dr: int) (dc: int) =
        let rec check (r: int) (c: int) =
            if r < 0 || r >= height || c < 0 || c >= width then false
            elif map.[r].[c] = '#' then false
            elif map.[r].[c] = 'O' then check (r + dr) (c + dc)
            else map.[r].[c] = '.'
        check row col

    let moveChain (row: int) (col: int) (dr: int) (dc: int) =
        let rec collectChain acc (r: int) (c: int) =
            if map.[r].[c] = 'O' then collectChain ((r, c) :: acc) (r + dr) (c + dc)
            else acc
        let chain = collectChain [] row col |> List.rev
        for (r, c) in chain do map.[r].[c] <- '.'
        for (r, c) in chain do map.[r + dr].[c + dc] <- 'O'

    let mutable robotRow, robotCol = findRobot()
    for move in moves do
        let dr, dc = directions.[move]
        let newRow, newCol = robotRow + dr, robotCol + dc
        if newRow >= 0 && newRow < height && newCol >= 0 && newCol < width then
            match map.[newRow].[newCol] with
            | '.' ->
                map.[robotRow].[robotCol] <- '.'
                map.[newRow].[newCol] <- '@'
                robotRow <- newRow
                robotCol <- newCol
            | 'O' ->
                if canMoveChain newRow newCol dr dc then
                    moveChain newRow newCol dr dc
                    map.[robotRow].[robotCol] <- '.'
                    map.[newRow].[newCol] <- '@'
                    robotRow <- newRow
                    robotCol <- newCol
            | _ -> ()

let calculateGPS (map: char[][]) =
    let mutable sum = 0
    for i in 0 .. map.Length - 1 do
        for j in 0 .. map.[0].Length - 1 do
            if map.[i].[j] = 'O' then sum <- sum + (100 * i + j)
    sum

let filePath = "input-2024-15.txt"
let map, moves = readInput filePath
moveRobot map moves
let result = calculateGPS map
printfn "%d" result
