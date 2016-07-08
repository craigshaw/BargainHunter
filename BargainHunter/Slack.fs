module Slack

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

let BotName = "Hunter"
let BotIcon = ":moneybag:"

// todo Make this a little more elegant

let createJsonPayload username emoji text =
    sprintf """{"username":"%s", "text":"%s", "icon_emoji":"%s"}""" username text emoji

let postToSlack webHookUri message  =
    try
        Http.RequestString(webHookUri, body = FormValues ["payload", createJsonPayload BotName BotIcon message])
    with
    | ex -> sprintf "Error: %s" ex.Message