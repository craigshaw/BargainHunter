// Learn more about F# at http://fsharp.org
// See the 'F# Tutorial' project for more help.
open System
open System.IO
open Domain
open Slack

let LastRunFile = "last"
let Version = "0.1.0"
let AppName = "BargainHunter"

let ApplicationName =
    sprintf "%s v%s" AppName Version

let getInputParameters argv =
    if Array.length argv = 3 then
        Some(argv.[0], argv.[1], argv.[2])
    else
        None

let printUsage () = 
    printfn "Usage:\n%s.exe <HUKD Key> <Search Term> <Publish Uri>" AppName

let printHeader lastRun search key = 
    printfn "%s\nLast run: %O\nSearching for '%s' with HUKD key %s\n" ApplicationName lastRun search key

let OneWeekAgo =
    DateTime.Now.AddDays(-7.0)

let getLastRunTime = 
    match File.Exists(LastRunFile) with
    | true -> File.ReadAllText(LastRunFile) |> int |> unixTimeToDateTime
    | _ -> OneWeekAgo

// todo put this behind a mailboxprocessor?
let publishDeals deals hook =
    let resp = 
     deals 
     |> Seq.map (fun d -> 
                  sprintf "<%s|%s>\nListed at %O for £%O" d.Link d.Title d.Listed d.Price)
     |> Seq.fold (fun state deal -> state + deal + "\n\n") ""
     |> createJsonPayload "Hunter" ":moneybag:"
     |> postToSlack hook

    printfn "Slack response: %s" resp

let findDeals key search hook =
    let lastRun = getLastRunTime
    let currentRun = DateTime.Now

    printHeader lastRun search key

    let deals = getDeals key search lastRun

    printDeals deals

    publishDeals deals hook

    File.WriteAllText(LastRunFile, string <| dateTimeToUnixTime currentRun)

[<EntryPoint>]
let main argv = 
    match getInputParameters argv with
    | Some(key, search, hook) -> findDeals key search hook
    | _ -> printUsage ()

    Console.ReadKey() |> ignore
    0 // return an integer exit code