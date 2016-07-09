module Lego

open Domain
open System.Text.RegularExpressions

type LegoDeal = {Deal: Deal; ManufacturerCode: string list}

let manufacturerCodeLocator = Regex(@"\b(10|11|21|30|31|40|41|42|44|45|60|66|70|71|75|76|79)\d{3}\b", RegexOptions.Compiled);

let legoDealPrinter deal =
    printfn "%s" deal.Deal.Title
    printfn "%s" deal.Deal.Category
    printfn "%s" deal.Deal.Link
    printfn "%O" deal.Deal.Price
    printfn "%O" deal.Deal.Listed
    printfn "%O" deal.ManufacturerCode

let extractManufacturerCode codes =
    sprintf "Product Code: %s" <|
        match codes with
        | (fst :: []) -> fst
        | (fst :: rst) -> sprintf "(%s)" <| String.concat "," codes 
        | _ -> "Unknown"

let (|ManufacturerCode|_|) (dealText:string) =
    let codes = manufacturerCodeLocator.Matches(dealText)

    if codes.Count > 0 then 
        Some ([for uniqueCode in [for code in codes -> code.Value] |> Seq.distinctBy id -> uniqueCode])
    else 
        None

let legoFilter (deal:HUKDProvider.Item) = 
    deal.Category.Name <> "Gaming" &&
    match deal.Price with 
    | Some _ -> true 
    | None -> false

let legoMap (deal:HUKDProvider.Item) =
    {LegoDeal.Deal = {Title =deal.Title; 
                      Link =deal.DealLink; 
                      Price = Option.get deal.Price; 
                      Listed = unixTimeToDateTime deal.Timestamp;
                      Category = deal.Category.Name;};
    LegoDeal.ManufacturerCode = match deal.Title with
                                | ManufacturerCode code -> code
                                | _ -> []}

let legoDealPublisherMap deal = 
    sprintf "<%s|%s>\nListed at %O for £%O\n%s" 
     deal.Deal.Link 
     deal.Deal.Title 
     deal.Deal.Listed 
     deal.Deal.Price 
     <| extractManufacturerCode deal.ManufacturerCode
