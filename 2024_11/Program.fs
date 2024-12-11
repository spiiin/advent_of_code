open System

let rec dfs (stone: bigint) (step: int) (totalBlinks: int) (memory: Map<(bigint * int), bigint>) : (bigint * Map<(bigint * int), bigint>) =
    if step = totalBlinks then (1I, memory)
    else
        match Map.tryFind (stone, step) memory with
        | Some value -> (value, memory)
        | None ->
            let result, updatedMemory =
                if stone = 0I then dfs 1I (step + 1) totalBlinks memory
                elif stone.ToString().Length % 2 = 0 then
                    let str = stone.ToString()
                    let mid = str.Length / 2
                    let left = bigint.Parse(str.Substring(0, mid))
                    let right = bigint.Parse(str.Substring(mid))
                    let leftResult, mem1 = dfs left (step + 1) totalBlinks memory
                    let rightResult, mem2 = dfs right (step + 1) totalBlinks mem1
                    (leftResult + rightResult, mem2)
                else dfs (stone * 2024I) (step + 1) totalBlinks memory
            (result, Map.add (stone, step) result updatedMemory)

let solve (input: string) (totalBlinks: int) : bigint =
    input.Split(' ')
    |> Array.map bigint.Parse
    |> Array.fold (fun (sum, memory) stone ->
        let result, updatedMemory = dfs stone 0 totalBlinks memory
        (sum + result, updatedMemory)) (0I, Map.empty)
    |> fst

let input = System.IO.File.ReadAllLines("input-2024-11.txt").[0]
printfn $"{solve input 25}\n{solve input 75}"