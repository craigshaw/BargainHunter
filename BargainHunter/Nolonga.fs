module Nolonga

open Domain

let ServiceUriTemplate = Printf.StringFormat<string->string>("http://www.nolonga.com/Api/PriceHistory/%s")

let getProductPriceHistory product =
    try
        let priceHistory = NolongaProvider.Load(sprintf ServiceUriTemplate product)
        Some(priceHistory)
    with _ -> None

