module Hukd

open Domain
open Time
open System.Web

let ServiceUriTemplate = Printf.StringFormat<string->int->string->string>("http://api.hotukdeals.com/rest_api/v2/?key=%s&output=json&forum=deals&exclude_expired=true&results_per_page=30&page=%d&search=%s")

let buildSearchUri (searchString:string) key page = 
    sprintf ServiceUriTemplate key page <| HttpUtility.UrlEncode(searchString)

let isNewerThan baseTime timestamp = 
    timestamp > baseTime

let isOlderThan baseTime timestamp = 
    not <| isNewerThan baseTime timestamp

let (|RelevantDeal|_|) (lastSearchTime:int, deal:HUKDProvider.Item, filter:HUKDProvider.Item -> bool) = 
    if deal.Timestamp |> isNewerThan lastSearchTime && filter deal then
        Some()
    else
        None

let getDeals filter codeResolver priceResolver key search lastSearchTime = 
    let rec loop deals currentPage = 
        let allDeals = HUKDProvider.Load(buildSearchUri search key currentPage)
        let relevantDeals = allDeals.Deals.Items 
                            |> Seq.filter (fun i -> 
                                            match lastSearchTime, i, filter with
                                            | RelevantDeal -> true
                                            | _ -> false)
                            |> Seq.map (fun i -> 
                                            {Title = i.Title; 
                                            Link = i.DealLink; 
                                            Price = (match i.Price with | Some price -> price | _ -> 0.0m);
                                            Listed = unixTimeToDateTime i.Timestamp;
                                            Category = i.Category.Name;
                                            ManufacturerCode = [];
                                            PriceDetails = None})
                            |> Seq.map (fun i -> codeResolver i)
                            |> Seq.map (fun i -> priceResolver i)
                            |> Seq.toList
                            |> List.append deals

        match (allDeals.Deals.Items |> Seq.last, currentPage)  with
        | (last,_) when last.Timestamp |> isOlderThan lastSearchTime -> relevantDeals
        | (_,currentPage) when currentPage < (allDeals.TotalResults / 30) -> loop relevantDeals (currentPage + 1) 
        | _ -> relevantDeals

    loop [] 1

