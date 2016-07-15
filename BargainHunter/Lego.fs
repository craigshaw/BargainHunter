module Lego

open Domain
open System.Text.RegularExpressions

let BotName = "BrickHunter"
let BotIcon = ":moneybag:"

let manufacturerCodeLocator = Regex(@"\b(10|11|21|30|31|40|41|42|44|45|60|66|70|71|75|76|79)\d{3}\b", RegexOptions.Compiled);

let getPublicationIdentity =
    (BotName, BotIcon)

let (|ManufacturerCode|_|) (dealText:string) =
    let codes = manufacturerCodeLocator.Matches(dealText)

    if codes.Count > 0 then 
        Some ([for uniqueCode in [for code in codes -> code.Value] |> Seq.distinctBy id -> uniqueCode])
    else 
        None

let manufacturerCodeResolver deal =
    let codes =
     match deal.Title with
     | ManufacturerCode codes -> codes
     | _ -> []

    {deal with ManufacturerCode = codes}

let priceResolver deal = 
    let averagePrice = 
     match deal.ManufacturerCode with
     | (fst :: []) -> 
                    try
                      let priceHistory = NolongaProvider.Load(sprintf "http://localhost:53346/Api/PriceHistory/%s" fst)
                      Some(priceHistory.AverageSalePrice)
                    with _ -> None
     | _ -> None

    {deal with AveragePrice = averagePrice}

let dealFilter (deal:HUKDProvider.Item) = 
    deal.Category.Name <> "Gaming" &&
    match deal.Price with 
    | Some _ -> true 
    | None -> false

let formatForPublication deal= 
    sprintf "<%s|%s>\nListed at %O for £%O\n%s\n%s" 
     deal.Link
     deal.Title 
     deal.Listed
     deal.Price
     (match deal.ManufacturerCode with
      | (fst :: []) -> fst
      | (fst :: rst) -> sprintf "(%s)" <| String.concat "," deal.ManufacturerCode 
      | _ -> "Unknown")
     (match deal.AveragePrice with
      | Some average -> sprintf "The average price for this on Nolonga is currently £%.2f" average
      | None -> "")
