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

let formatForPublication deal = 
    sprintf "<%s|%s>\nListed at %O for £%O" 
     deal.Link 
     deal.Title 
     deal.Listed
     deal.Price
