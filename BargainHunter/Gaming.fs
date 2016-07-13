module Gaming

open Domain
open System.Text.RegularExpressions

let BotName = "GameHunter"
let BotIcon = ":video_game:"

let getPublicationIdentity =
    (BotName, BotIcon)

let dealFilter (deal:HUKDProvider.Item) = 
    deal.Category.Name = "Gaming" &&
    match deal.Price with 
    | Some _ -> true 
    | None -> false

let formatForPublication (deal:HUKDProvider.Item) = 
    sprintf "<%s|%s>\nListed at %O for £%O" 
     deal.DealLink 
     deal.Title 
     (unixTimeToDateTime deal.Timestamp)
     (match deal.Price with
      | Some price -> price
      | None -> 0.0m)
