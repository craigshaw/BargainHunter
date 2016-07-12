module Domain

open System
open System.IO
open System.Web
open FSharp.Data

type Domain =
| Lego
| Gaming

type HUKDProvider = JsonProvider<"Data/example.json">

let unixTimeToDateTime (unixTime:int) =
    DateTimeOffset.FromUnixTimeSeconds(int64 unixTime).DateTime.ToLocalTime()

let dateTimeToUnixTime (dt:DateTime) =
    DateTimeOffset(dt).ToUnixTimeSeconds() |> int


let (|SupportedDealDomain|_|) arg =
    if String.Compare("lego", arg, StringComparison.OrdinalIgnoreCase) = 0 then
        Some(Lego)
    else if String.Compare("gaming", arg, StringComparison.OrdinalIgnoreCase) = 0 then
        Some(Gaming)
    else
        None

let buildSearchUri (searchString:string) key page = 
    sprintf 
        "http://api.hotukdeals.com/rest_api/v2/?key=%s&output=json&forum=deals&exclude_expired=true&results_per_page=30&page=%d&search=%s" 
        key
        page 
        <| HttpUtility.UrlEncode(searchString)

let isNewerThan baseTime timestamp = 
    timestamp > baseTime

let isOlderThan baseTime timestamp = 
    not <| isNewerThan baseTime timestamp

let (|RelevantDeal|_|) (lastSearchTime:int, deal:HUKDProvider.Item, filter:HUKDProvider.Item -> bool) = 
    if deal.Timestamp |> isNewerThan lastSearchTime && filter deal then
        Some()
    else
        None

let getDeals filter key search lastSearchTime = 
    let rec loop deals currentPage = 
        let allDeals = HUKDProvider.Load(buildSearchUri search key currentPage)
        let relevantDeals = allDeals.Deals.Items 
                            |> Seq.filter (fun i -> 
                                            match lastSearchTime, i, filter with
                                            | RelevantDeal -> true
                                            | _ -> false)
                            |> Seq.toList
                            |> List.append deals

        match (allDeals.Deals.Items |> Seq.last, currentPage)  with
        | (last,_) when last.Timestamp |> isOlderThan lastSearchTime -> relevantDeals
        | (_,currentPage) when currentPage < (allDeals.TotalResults / 30) -> loop relevantDeals (currentPage + 1) 
        | _ -> relevantDeals

    loop [] 1

