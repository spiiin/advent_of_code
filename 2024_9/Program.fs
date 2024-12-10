open System
open System.IO

let parseDiskMap (diskMap: string) =
    let rec parseRec chars id acc =
        match chars with
        | [] -> Array.concat (List.rev acc)
        | x::rest ->
            let fileBlocks = Array.create (int (string x)) (char (id + int '0'))
            match rest with
            | [] -> parseRec [] (id + 1) (fileBlocks :: acc)
            | y::remaining ->
                let freeBlocks = Array.create (int (string y)) '.'
                parseRec remaining (id + 1) ((Array.append fileBlocks freeBlocks) :: acc)
    parseRec (Seq.toList diskMap) 0 []

let compactDisk blocks =
    let blocksArray = blocks |> Array.copy
    let rec moveBlockRightToLeft lastIndex =
        if lastIndex < 0 then ()
        else
            let freeIndex = Array.tryFindIndex ((=) '.') blocksArray
            match freeIndex with
            | Some i when i < lastIndex ->
                blocksArray.[i] <- blocksArray.[lastIndex]
                blocksArray.[lastIndex] <- '.'
                moveBlockRightToLeft (lastIndex - 1)
            | _ -> moveBlockRightToLeft (lastIndex - 1)
    moveBlockRightToLeft (blocksArray.Length - 1)
    blocksArray

let compactDiskWholeFilesOptimized blocks =
    let blocksArray = blocks |> Array.copy
    let fileIds =
        blocksArray
        |> Array.choose (fun c -> if c <> '.' then Some (int c - int '0') else None)
        |> Array.distinct
        |> Array.sortDescending

    let mutable freeSpans =
        let spans = ResizeArray()
        let mutable start = -1
        let mutable length = 0
        for i = 0 to blocksArray.Length - 1 do
            if blocksArray.[i] = '.' then
                if start = -1 then start <- i
                length <- length + 1
            elif length > 0 then
                spans.Add((start, length))
                start <- -1
                length <- 0
        if length > 0 then spans.Add((start, length))
        spans |> Seq.toArray

    for fileId in fileIds do
        let fileChar = char (fileId + int '0')
        let filePositions = Array.indexed blocksArray |> Array.filter (fun (_, v) -> v = fileChar)
        if filePositions.Length > 0 then
            let fileLength = filePositions.Length
            let leftmostPosition = fst filePositions.[0]
            let mutable found = false

            for i = 0 to freeSpans.Length - 1 do
                if not found then
                    let (start, len) = freeSpans.[i]
                    if start + len <= leftmostPosition && len >= fileLength then
                        for j in 0 .. fileLength - 1 do
                            blocksArray.[start + j] <- fileChar
                        for (pos, _) in filePositions do
                            blocksArray.[pos] <- '.'
                        freeSpans.[i] <- (start + fileLength, len - fileLength)
                        if snd freeSpans.[i] = 0 then
                            freeSpans <- Array.filter (fun (s, l) -> l > 0) freeSpans
                        found <- true
    blocksArray


let calculateChecksum blocks =
    blocks
    |> Array.mapi (fun i v -> if v <> '.' then int64 i * int64 (int v - int '0') else 0L)
    |> Array.sum

let arrayToString (arr: 'a array) =
    arr |> Array.map string |> String.concat ""

let processDiskMap diskMap useWholeFiles =
    try
        let initialBlocks = parseDiskMap diskMap
        let compactedBlocks =
            if useWholeFiles then compactDiskWholeFilesOptimized initialBlocks
            else compactDisk initialBlocks
        let checksum = calculateChecksum compactedBlocks
        checksum
    with
    | ex -> 
        printfn $"Error: {ex.Message}"
        -1L

let main () =
    let inputPath = "input-2024-9.txt"
    let diskMap = File.ReadAllText(inputPath).Trim()
    let partOneChecksum = processDiskMap diskMap false
    printfn $"Checksum (Part One): {partOneChecksum}"
    let partTwoChecksum = processDiskMap diskMap true
    printfn $"Checksum (Part Two): {partTwoChecksum}"

main ()
0