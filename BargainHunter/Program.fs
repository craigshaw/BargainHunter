open System
open System.IO
open Domain
open Slack
open Lego

let LastRunFile = "last"
let Version = "0.3.0"
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
    printfn "%s\nLast run: %O\nSearching for '%s' with HUKD key %s\n" ApplicationName (unixTimeToDateTime lastRun) search key

let OneWeekAgo =
    DateTime.Now.AddDays(-7.0) |> dateTimeToUnixTime

let getLastRunTime = 
    match File.Exists(LastRunFile) with
    | true -> File.ReadAllText(LastRunFile) |> int
    | _ -> OneWeekAgo

// todo put this behind a mailboxprocessor?
let publishDeals deals hook dealMapper =
    let resp = 
     deals 
     |> Seq.map dealMapper
     |> Seq.fold (fun state deal -> state + deal + "\n\n") ""
     |> postToSlack hook

    printfn "Slack response: %s" resp

let printDeals deals dealPrinter =
    deals |> Seq.iter (fun d -> dealPrinter d)

let findDeals key search hook =
    let lastRun = getLastRunTime
    let currentRun = DateTime.Now

    printHeader lastRun search key

    let deals = getDeals key search lastRun legoFilter legoMap

    match deals.Length with
    | 0 -> printfn "No new deals found"
    | _ -> printDeals deals legoDealPrinter
           publishDeals deals hook legoDealPublisherMap

    File.WriteAllText(LastRunFile, string <| dateTimeToUnixTime currentRun)

[<EntryPoint>]
let main argv = 
    match getInputParameters argv with
    | Some(key, search, hook) -> findDeals key search hook
    | _ -> printUsage ()

#if DEBUG
    Console.ReadKey() |> ignore
#endif
    0 // return an integer exit code