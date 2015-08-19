module Listener

open FSharp.Data
open FSharp.Configuration
open Nessos.FsPickler
open Metrics
open WebSocketSharp
open WebSocketSharp.Server

type Settings = AppSettings<"app.config">

type AmplitudeService () =
    inherit WebSocketBehavior ()

    let pickler = FsPickler.CreateBinary ()

    override self.OnMessage (e:MessageEventArgs) = 
        let id, event = self.UnPickle e.RawData
        self.SendRequest id event
        
    member self.SendRequest id event = 
        printfn "sending to amplitude"
        let url = "https://api.amplitude.com/httpapi"
        let api_key = Settings.ApiKey
        let event = "[{\"user_id\":\"" + id + "\",\"event_type\":\"" + event + "\"}]"
        let requestBody = FormValues[("api_key", api_key); ("event", event)]
        Http.AsyncRequestString(url, body = requestBody) 
        //|> Async.RunSynchronously |> printfn "Request status: %A"
        |> Async.Ignore |> Async.Start

    member self.UnPickle pickle = 
        pickler.UnPickle<string*string> pickle

let startListening () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<AmplitudeService>("/AmplitudeService")
    wsServer.Start ()