open System
open System.IO
open Domain
open Time
open Hukd
open Slack
open Lego

let Version = "0.6.0"
let AppName = "BargainHunter"

let ApplicationName =
    sprintf "%s v%s" AppName Version

let getInputParameters argv =
    if Array.length argv = 5 then
        match argv.[1] with
        | SupportedDealDomain domain -> Some(argv.[0], domain, argv.[2], argv.[3], argv.[4])
        | _ -> None
    else
        None

let printUsage () = 
    printfn "Usage:\n%s.exe <Search Term> <Domain> <Last Run File> <HUKD Key> <Publish Uri>" AppName

let printHeader lastRun search key = 
    printfn "%s\nLast run: %O\nSearching for '%s' with HUKD key %s\n" ApplicationName (unixTimeToDateTime lastRun) search key

let OneWeekAgo =
    DateTime.Now.AddDays(-7.0) |> dateTimeToUnixTime

let getLastRunTimeFromFile file = 
    match File.Exists(file) with
    | true -> File.ReadAllText(file) |> int
    | _ -> OneWeekAgo

// todo put this behind a mailboxprocessor?
let publishDeals publicationFormatter publicationIdentity deals hook =
    let response = 
     deals 
     |> Seq.map publicationFormatter
     |> Seq.fold (fun state deal -> state + deal + "\n\n") ""
     |> postToSlack publicationIdentity hook

    printfn "Slack response: %s" response

let bootstrap domain =
    // Partially apply the resolver and publisher functions
    match domain with
    | Lego -> (getDeals Lego.dealFilter Lego.manufacturerCodeResolver Lego.priceResolver,
               publishDeals Lego.formatForPublication Lego.getPublicationIdentity)
    | Gaming -> (getDeals Gaming.dealFilter (fun d -> d) (fun d -> d),
                 publishDeals Gaming.formatForPublication Gaming.getPublicationIdentity)

let findDeals key search hook domain lastRunFile =
    let getDeals', publishDeals' = bootstrap domain
    let lastRun = getLastRunTimeFromFile lastRunFile
    let currentRun = DateTime.Now

    printHeader lastRun search key

    let deals = getDeals' key search lastRun

    match deals.Length with
    | 0 -> printfn "No new deals found"
    | n -> printfn "%d new deals found" n
           publishDeals' deals hook

    File.WriteAllText(lastRunFile, string <| dateTimeToUnixTime currentRun)

[<EntryPoint>]
let main argv = 
    match getInputParameters argv with
    | Some(search, domain, lastRunFile, key, hook) -> findDeals key search hook domain lastRunFile
    | _ -> printUsage ()

#if DEBUG
    Console.ReadKey() |> ignore
#endif
    0 // return an integer exit code