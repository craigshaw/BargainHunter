module Lego

open Domain
open System.Text.RegularExpressions

type LegoDeal = {Deal: Deal; ManufacturerCode: string list}

let manufacturerCodeLocator = Regex(@"\b(10|11|21|30|31|40|41|42|44|45|60|66|70|71|75|76|79)\d{3}\b", RegexOptions.Compiled);

let printLegoDeals deals =
    deals 
    |> Seq.iter (fun i -> 
        printfn "%s" i.Deal.Title
        printfn "%s" i.Deal.Category
        printfn "%s" i.Deal.Link
        printfn "%O" i.Deal.Price
        printfn "%O" i.Deal.Listed
        printfn "%O" i.ManufacturerCode)

let extractManufacturerCode codes =
    sprintf "Product Code: %s" <|
        match codes with
        | (fst :: []) -> fst
        | (fst :: rst) -> sprintf "(%s)" <| String.concat "," codes 
        | _ -> "Unknown"

let legoFilter (deal:HUKDProvider.Item) = 
    deal.Category.Name <> "Gaming" &&
    match deal.Price with 
    | Some _ -> true 
    | None -> false

let (|ManufacturerCode|_|) (dealText:string) =
    let codes = manufacturerCodeLocator.Matches(dealText)

    if codes.Count > 0 then 
        Some ([for uniqueCode in [for code in codes -> code.Value] |> Seq.distinctBy id -> uniqueCode])
    else 
        None

let identifyProducts deals =
    deals 
    |> Seq.map (fun d -> 
                match d.Title with
                | ManufacturerCode code -> { Deal = d; ManufacturerCode = code }
                | _ -> { Deal = d; ManufacturerCode = [] })