module Domain

open System
open System.IO
open System.Web
open FSharp.Data

type HUKDProvider = JsonProvider<"Data/example.json">

type Deal = { Title: string; Link: string; Price: decimal; Listed: DateTime; Category: string }

let printDeals deals =
    deals 
    |> Seq.iter (fun i -> 
        printfn "%s" i.Title
        printfn "%s" i.Category
        printfn "%s" i.Link
        printfn "%O" i.Price
        printfn "%O\n" i.Listed)

let dateTimeToUnixTime (dt:DateTime) =
    let epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
    let span = (dt.ToLocalTime() - epoch)
    int span.TotalSeconds

let unixTimeToDateTime (ut:int) =
    let epoch = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc)
    epoch.AddSeconds(float ut).ToLocalTime()

let buildSearchUri (searchString:string) key page = 
    sprintf 
        "http://api.hotukdeals.com/rest_api/v2/?key=%s&output=json&forum=deals&exclude_expired=true&results_per_page=30&page=%d&search=%s" 
        key
        page 
        <| HttpUtility.UrlEncode(searchString)

let isNewerThan baseTime timestamp = 
    DateTime.Compare((unixTimeToDateTime timestamp), baseTime) > 0

let isOlderThan baseTime timestamp = 
    not <| isNewerThan baseTime timestamp

let (|RelevantDeal|_|) (lastSearchTime:DateTime, deal:HUKDProvider.Item) = 
    if deal.Timestamp |> isNewerThan lastSearchTime &&
        deal.Category.Name <> "Gaming" &&
        match deal.Price with 
        | Some _ -> true 
        | None -> false
    then
        Some()
    else
        None

let getDeals key search lastSearchTime = 
    let rec loop deals currentPage = 
        let allDeals = HUKDProvider.Load(buildSearchUri search key currentPage)
        let relevantDeals = allDeals.Deals.Items 
                            |> Seq.filter (fun i -> 
                                            match lastSearchTime, i with
                                            | RelevantDeal -> true
                                            | _ -> false)
                            |> Seq.map (fun i -> 
                                            {Title = i.Title; 
                                            Link = i.DealLink; 
                                            Price = Option.get i.Price; 
                                            Listed = unixTimeToDateTime i.Timestamp;
                                            Category = i.Category.Name })
                            |> Seq.toList
                            |> List.append deals

        match (allDeals.Deals.Items |> Seq.last, currentPage)  with
        | (last,_) when last.Timestamp |> isOlderThan lastSearchTime -> relevantDeals
        | (_,currentPage) when currentPage < (allDeals.TotalResults / 30) -> loop relevantDeals (currentPage + 1) 
        | _ -> relevantDeals

    loop [] 1

