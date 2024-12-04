open System.IO
open System.Text.RegularExpressions

let parseAndSumConditionalMultiplications (input: string) =
    let mulPattern = @"mul\((\d+),(\d+)\)"
    let doPattern = @"do\(\)"
    let dontPattern = @"don't\(\)"
    
    let mutable isEnabled = true
    let mutable sum = 0
    
    let instructions = Regex.Matches(input, $"{mulPattern}|{doPattern}|{dontPattern}")
    
    for instruction in instructions do
        let text = instruction.Value
        match text with
        | _ when Regex.IsMatch(text, doPattern) ->
            isEnabled <- true
        | _ when Regex.IsMatch(text, dontPattern) ->
            isEnabled <- false
        | _ when Regex.IsMatch(text, mulPattern) && isEnabled ->
            let matchGroups = Regex.Match(text, mulPattern).Groups
            let x = int matchGroups.[1].Value
            let y = int matchGroups.[2].Value
            sum <- sum + (x * y)
        | _ -> ()
    sum

let corruptedMemory = File.ReadAllText("input-2024-3.txt")
printfn $"{parseAndSumConditionalMultiplications corruptedMemory}"