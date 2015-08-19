module Listener

open FSharp.Data
open FSharp.Configuration
open Nessos.FsPickler
open Metrics
open WebSocketSharp

type Settings = AppSettings<"app.config">

let sendRequest id event = 
    let url = "https://api.amplitude.com/httpapi"
    let api_key = Settings.ApiKey
    let event = "[{\"user_id\":\"" + id + "\",\"event_type\":\"" + event + "\"}]"
    let requestBody = FormValues[("api_key", api_key); ("event", event)]
    Http.AsyncRequestString(url, body = requestBody) 
    //|> Async.RunSynchronously |> printfn "Request status: %A"
    |> Async.Ignore |> Async.Start

//let unPickle pickle = 
//    let pickler = FsPickler.CreateBinarySerializer ()
//    let id, event = pickler.UnPickle<string*string> pickle
//    id, event

//let recieve pickle =
//    let id, event = unPickle pickle
//    sendRequest id event