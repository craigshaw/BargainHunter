// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.IO
open Domain

let LastRunFile = "last"
let Version = "0.1.0"
let AppName = "BargainHunter"

let ApplicationName =
    sprintf "%s v%s" AppName Version

let getInputParameters argv =
    if Array.length argv = 2 then
        Some(argv.[0], argv.[1])
    else
        None

let printUsage () = 
    printfn "Usage:\n%s.exe <HUKD key> <search term>" AppName

let printHeader lastRun search key = 
    printfn "%s\nLast run: %O\nSearching for '%s' with HUKD key %s\n" ApplicationName lastRun search key

let OneWeekAgo =
    DateTime.Now.AddDays(-7.0)

let getLastRunTime = 
    match File.Exists(LastRunFile) with
    | true -> File.ReadAllText(LastRunFile) |> int |> unixTimeToDateTime
    | _ -> OneWeekAgo

let findDeals key search =
    let lastRun = getLastRunTime
    let currentRun = DateTime.Now

    printHeader lastRun search key

    let deals = getDeals key search lastRun

    printDeals deals

    File.WriteAllText(LastRunFile, string <| dateTimeToUnixTime currentRun)

[<EntryPoint>]
let main argv = 
    match getInputParameters argv with
    | Some(key, search) -> findDeals key search
    | _ -> printUsage ()

    Console.ReadKey() |> ignore
    0 // return an integer exit code