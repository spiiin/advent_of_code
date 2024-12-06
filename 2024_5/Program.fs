open System
open System.IO

let parseRules (lines: string list) =
    lines
    |> List.map (fun line ->
        let parts = line.Split('|')
        int parts.[0], int parts.[1])

let parseUpdates (lines: string list) =
    lines |> List.map (fun line -> line.Split(',') |> Array.map int |> Array.toList)

let validateUpdate (rules: (int * int) list) (update: int list) =
    let indices = update |> List.mapi (fun i x -> x, i) |> Map.ofList
    rules
    |> List.forall (fun (x, y) ->
        match Map.tryFind x indices, Map.tryFind y indices with
        | Some ix, Some iy -> ix < iy
        | _ -> true)

let middlePage (update: int list) =
    update.[update.Length / 2]

let reorderUpdate (rules: (int * int) list) (update: int list) =
    let dependencies = 
        rules 
        |> List.filter (fun (x, y) -> List.contains x update && List.contains y update)
        |> List.groupBy fst
        |> Map.ofList
    let visited = System.Collections.Generic.HashSet<int>()
    let result = System.Collections.Generic.List<int>()
    let inProcess = System.Collections.Generic.HashSet<int>()
    
    let rec visit node =
        if visited.Contains(node) then ()
        elif inProcess.Contains(node) then
            failwithf "Cyclic dependency detected involving page %d" node
        else
            inProcess.Add(node) |> ignore
            match Map.tryFind node dependencies with
            | Some dependents ->
                dependents
                |> List.map snd
                |> List.iter visit
            | None -> ()
            inProcess.Remove(node) |> ignore
            visited.Add(node) |> ignore
            result.Add(node)

    update |> List.iter visit
    result |> Seq.toList

let processInput (rules: (int * int) list) (updates: int list list) =
    updates
    |> List.filter (validateUpdate rules)
    |> List.map middlePage
    |> List.sum

let processInputPartTwo (rules: (int * int) list) (updates: int list list) =
    let incorrectlyOrdered = 
        updates |> List.filter (fun update -> not (validateUpdate rules update))
    incorrectlyOrdered
    |> List.map (reorderUpdate rules)
    |> List.map middlePage
    |> List.sum

let span predicate (list: 'a list) =
    let before = list |> List.takeWhile predicate
    let after = list |> List.skipWhile predicate
    before, after

let readInput fileName =
    let lines = File.ReadAllLines(fileName) |> Array.toList
    let ruleLines, updateLines =
        lines |> span (fun line -> line.Contains("|"))
    let rules = parseRules ruleLines
    let updates = parseUpdates (List.tail updateLines)
    rules, updates

[<EntryPoint>]
let main argv =
    let rules, updates = readInput "input-2024-5.txt"
    let resultPartOne = processInput rules updates
    printfn $"The sum of middle page numbers from the file (Part One): {resultPartOne}" 
    let resultPartTwo = processInputPartTwo rules updates
    printfn $"The sum of middle page numbers from corrected updates (Part Two): {resultPartTwo}" 
    0
