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

let extractManufacturerCode (deal:HUKDProvider.Item) =
    let manufacturerCodes = match deal.Title with
                            | ManufacturerCode codes -> codes
                            | _ -> [] 

    sprintf "Product Code: %s" <|
        match manufacturerCodes with
        | (fst :: []) -> fst
        | (fst :: rst) -> sprintf "(%s)" <| String.concat "," manufacturerCodes 
        | _ -> "Unknown"

let dealFilter (deal:HUKDProvider.Item) = 
    deal.Category.Name <> "Gaming" &&
    match deal.Price with 
    | Some _ -> true 
    | None -> false

let formatForPublication (deal:HUKDProvider.Item) = 
    sprintf "<%s|%s>\nListed at %O for £%O\n%s" 
     deal.DealLink 
     deal.Title 
     (unixTimeToDateTime deal.Timestamp)
     (match deal.Price with
      | Some price -> price
      | None -> 0.0m)
     <| extractManufacturerCode deal
