module Slack

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

// todo Make this a little more elegant

let escapeQuotes (str:string) =
    str.Replace("\"", "\\\"")

let createJsonPayload username emoji text =
    sprintf """{"username":"%s", "text":"%s", "icon_emoji":"%s"}""" username (escapeQuotes text) emoji

let postToSlack publicationIdentity webHookUri message  =
    try
        Http.RequestString(webHookUri, body = FormValues ["payload", createJsonPayload (fst publicationIdentity) (snd publicationIdentity) message])
    with
    | ex -> sprintf "Error: %s" ex.Message

