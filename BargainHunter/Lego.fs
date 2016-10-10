module Lego

open Domain
open Nolonga
open System.Text.RegularExpressions

let BotName = "BrickHunter"
let BotIcon = ":moneybag:"

let manufacturerCodeLocator = Regex(@"\b(10|11|21|30|31|40|41|42|44|45|60|66|70|71|75|76|79)\d{3}\b", RegexOptions.Compiled);

let getPublicationIdentity =
    (BotName, BotIcon)

let (|ManufacturerCode|_|) dealText =
    let codes = manufacturerCodeLocator.Matches(dealText)

    if codes.Count > 0 then 
        Some ([for uniqueCode in [for code in codes -> code.Value] |> Seq.distinctBy id -> uniqueCode])
    else 
        None

let manufacturerCodeResolver deal =
    {deal with ManufacturerCode = 
               match deal.Title with
               | ManufacturerCode codes -> codes
               | _ -> []}

let priceResolver deal = 
    // Only interested in the case where a single product code has been identified
    {deal with PriceDetails = 
               match deal.ManufacturerCode with
               | (fst :: []) -> getProductPriceHistory fst
               | _ -> None}

let dealFilter (deal:HUKDProvider.Item) = 
    deal.Category.Name <> "Gaming" &&
    match deal.Price with 
    | Some _ -> true 
    | None -> false

let percentageToSavingText =  function
    | n when n < 0.0m -> "more" | _ -> "less"

let averagePriceText productid avgPrice =
    sprintf "The average price for this on <http://www.nolonga.com/product/%d|Nolonga.com> is currently £%.2f" 
            productid avgPrice

let relativePercentageText percentageSaving = 
    sprintf "That's %.2f%% %s than the average" <| abs percentageSaving <| percentageToSavingText percentageSaving

let addPriceInfo deal = 
    match deal.PriceDetails with
    | Some details -> match details.PriceStats.NumberOfResults with
                      | 0 -> "There are no prices available for this product on <http://www.nolonga.com|Nolonga.com>"
                      | n -> let percentageSaving = (1.0m - (deal.Price / details.PriceStats.AverageSalePrice)) * 100.0m
                             sprintf "%s\n%s"
                             <| averagePriceText details.ProductId details.PriceStats.AverageSalePrice
                             <| relativePercentageText percentageSaving
    | None -> match deal.ManufacturerCode with
              | (fst :: []) -> sprintf "There is no record for this product (%s) on <http://www.nolonga.com|Nolonga.com>" fst
              | _ -> ""

let formatForPublication deal = 
    sprintf "<%s|%s>\nListed at %O for £%O\n%s" 
     deal.Link
     deal.Title 
     deal.Listed
     deal.Price
     <| addPriceInfo deal
