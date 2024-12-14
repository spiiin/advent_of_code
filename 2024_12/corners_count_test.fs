let readGridFromString (input: string) =
    input.Split('\n')
    |> Array.map (fun line -> line.ToCharArray() |> Array.toList)
    |> Array.toList

let countSymbols grid =
    let rows, cols = List.length grid, if grid <> [] then List.length (List.head grid) else 0

    let getValue x y =
        if x >= 0 && x < rows && y >= 0 && y < cols then Some (List.item x grid |> List.item y) else None

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
            printfn "Checking quad: (%A, %A, %A, %A)" x11 x12 x21 x22
            if x11 = x12 && x11 = x21 && x11 = x22 then
                ([], "All identical, skipped")
            else
                let results =
                    [ if x11 = x12 && x11 = x21 || x11 <> x12 && x11 <> x21 then x11 else None
                      if x12 = x11 && x12 = x22 || x12 <> x11 && x12 <> x22 then x12 else None
                      if x21 = x11 && x21 = x22 || x21 <> x11 && x21 <> x22 then x21 else None
                      if x22 = x12 && x22 = x21 || x22 <> x12 && x22 <> x21 then x22 else None ]
                printfn "Results for quad: %A" results
                (results, "Processed")
        ]
    |> List.choose (fun (results, status) -> if results = [] then None else Some results)
    |> List.concat
    |> List.fold updateCount Map.empty

let inputString = """OOOOO
OXOXO
OOOOO
OXOXO
OOOOO"""

let exampleGrid = readGridFromString inputString

let symbolCounts = countSymbols exampleGrid
symbolCounts |> Map.iter (fun key value -> printfn "%c: %d" key value)