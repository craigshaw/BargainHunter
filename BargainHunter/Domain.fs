module Domain

open System
open FSharp.Data

type HUKDProvider = JsonProvider<"Data/example.json">
type NolongaProvider = JsonProvider<"Data/nolonga.json">

type DealDomain =
| Lego
| Gaming

type Deal = { Title: string; Link: string; Price: decimal; Listed: DateTime; Category: string; 
              ManufacturerCode: string list; PriceDetails: NolongaProvider.Root option }

let (|SupportedDealDomain|_|) domain =
    if String.Compare("lego", domain, StringComparison.OrdinalIgnoreCase) = 0 then
        Some(Lego)
    else if String.Compare("gaming", domain, StringComparison.OrdinalIgnoreCase) = 0 then
        Some(Gaming)
    else
        None