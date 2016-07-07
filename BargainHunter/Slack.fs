module Slack

open FSharp.Data
open FSharp.Data.HttpRequestHeaders

// todo Make this a little more elegant

let postToSlack webHookUri payload =
    Http.RequestString
        ( webHookUri, 
        body = FormValues ["payload", payload])

let createJsonPayload username emoji text =
    sprintf """{"username":"%s", "text":"%s", "icon_emoji":"%s"}""" username text emoji

