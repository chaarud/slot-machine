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
        let id, metric = self.UnPickle e.RawData
        self.GenerateEvent id metric
        |> self.SendRequest (id.ToString())

    member self.GenerateEvent id (metric : Metric) = 
         let id = "[{\"user_id\":\"" + (id.ToString ()) + "\","
         let eventType = "\"event_type\":\"" + (self.EventName metric) + "\""
         id + eventType + (self.GenerateTail metric)

    member self.EventName = function
        | GameStarted _ -> "Game Started"
        | GameEnded -> "Game Ended"
        | BuyMoneyMetric _ -> "Bought Virtual Currency"
        | PullLeverMetric _ -> "Pulled Lever"

    member self.GenerateTail metric = 
        let contents = 
            match metric with
            | GameStarted platform -> ",\"platform\":\"" + platform + "\"" // ,"platform":"Android"
            | GameEnded -> ""
            | BuyMoneyMetric _ -> ""
            | PullLeverMetric _ -> ""
        contents + "}]"

    member self.SendRequest id event = 
        printfn "sending to amplitude"
        let url = "https://api.amplitude.com/httpapi"
        let api_key = Settings.ApiKey
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