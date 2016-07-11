open System
open System.IO
open Domain
open Slack
open Lego

type Domain =
| Lego
//| Gaming

let LastRunFile = "last"
let Version = "0.3.0"
let AppName = "BargainHunter"

let ApplicationName =
    sprintf "%s v%s" AppName Version

let (|SupportedDealDomain|_|) arg =
    if String.Compare("lego", arg, StringComparison.OrdinalIgnoreCase) = 0 then
        Some(Lego)
//    else if String.Compare("gaming", arg, StringComparison.OrdinalIgnoreCase) = 0 then
//        Some(Gaming)
    else
        None

let getInputParameters argv =
    if Array.length argv = 4 then
        match argv.[1] with
        | SupportedDealDomain domain -> Some(argv.[0], domain, argv.[2], argv.[3])
        | _ -> None
    else
        None

let printUsage () = 
    printfn "Usage:\n%s.exe <Search Term> <Domain> <HUKD Key> <Publish Uri>" AppName

let printHeader lastRun search key = 
    printfn "%s\nLast run: %O\nSearching for '%s' with HUKD key %s\n" ApplicationName (unixTimeToDateTime lastRun) search key

let OneWeekAgo =
    DateTime.Now.AddDays(-7.0) |> dateTimeToUnixTime

let getLastRunTime = 
    match File.Exists(LastRunFile) with
    | true -> File.ReadAllText(LastRunFile) |> int
    | _ -> OneWeekAgo

// todo put this behind a mailboxprocessor?
let publishDealsToSlack publicationFormatter publicationIdentity deals hook =
    let resp = 
     deals 
     |> Seq.map publicationFormatter
     |> Seq.fold (fun state deal -> state + deal + "\n\n") ""
     |> postToSlack publicationIdentity hook

    printfn "Slack response: %s" resp

let outputDeals dealPrinter deals =
    deals |> Seq.iter (fun d -> dealPrinter d)

let bootstrap domain =
    // Partially apply the resolver, writer and publisher functions
    match domain with
    | Lego -> (getDeals Lego.dealFilter Lego.dealMapper,
               outputDeals Lego.writeToConsole,
               publishDealsToSlack Lego.formatForPublication Lego.getPublicationIdentity)

let findDeals key search hook domain =
    let (resolveDeals, printDeals, publishDeals) = bootstrap domain
    let lastRun = getLastRunTime
    let currentRun = DateTime.Now

    printHeader lastRun search key

    let deals = resolveDeals key search lastRun

    match deals.Length with
    | 0 -> printfn "No new deals found"
    | _ -> printDeals deals
           publishDeals deals hook

    File.WriteAllText(LastRunFile, string <| dateTimeToUnixTime currentRun)

[<EntryPoint>]
let main argv = 
    match getInputParameters argv with
    | Some(search, domain, key, hook) -> findDeals key search hook domain
    | _ -> printUsage ()

#if DEBUG
    Console.ReadKey() |> ignore
#endif
    0 // return an integer exit code