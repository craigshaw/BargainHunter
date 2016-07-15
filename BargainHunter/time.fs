module Time

open System

let unixTimeToDateTime (unixTime:int) =
    DateTimeOffset.FromUnixTimeSeconds(int64 unixTime).DateTime.ToLocalTime()

let dateTimeToUnixTime (dt:DateTime) =
    DateTimeOffset(dt).ToUnixTimeSeconds() |> int