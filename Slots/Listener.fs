module Listener

open Account
open Metric

open FSharp.Data
open FSharp.Configuration
open Nessos.FsPickler
open WebSocketSharp
open WebSocketSharp.Server

type Settings = AppSettings<"app.config">

type AmplitudeService () =
    inherit WebSocketBehavior ()

    let pickler = FsPickler.CreateBinary ()

    override self.OnMessage (e:MessageEventArgs) = 
        let id, event = self.UnPickle e.RawData
        match event with
        | GameStarted _ -> "Game Started"
        | GameEnded -> "Game Ended"
        | BuyMoneyMetric _ -> "Bought virtual currency"
        | PullLeverMetric _ -> "Pulled Lever"
        |> self.SendRequest (id.ToString())

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
        pickler.UnPickle<int*Metric> pickle

let startListening () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<AmplitudeService>("/AmplitudeService")
    wsServer.Start ()