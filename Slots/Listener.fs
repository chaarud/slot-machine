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

    override self.OnMessage (e:MessageEventArgs) = 
        printfn "heard the message"
        let id, event = self.UnPickle e.RawData
        self.SendRequest id event

    override self.OnOpen () = 
        printfn "umm...websocketbehavior opening"

    member self.SendRequest id event = 
        let url = "https://api.amplitude.com/httpapi"
        let api_key = Settings.ApiKey
        let event = "[{\"user_id\":\"" + id + "\",\"event_type\":\"" + event + "\"}]"
        let requestBody = FormValues[("api_key", api_key); ("event", event)]
        Http.AsyncRequestString(url, body = requestBody) 
        //|> Async.RunSynchronously |> printfn "Request status: %A"
        |> Async.Ignore |> Async.Start

    member self.UnPickle pickle = 
        let pickler = FsPickler.CreateBinary ()
        let id, event = pickler.UnPickle<string*string> pickle
        id, event

//let recieve pickle =
//    let id, event = unPickle pickle
//    sendRequest id event

//let wsServer = new WebSocketServer(55555)
let startListener () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<AmplitudeService>("/AmplitudeService")
    wsServer.Start ()
    printfn "%A" wsServer.Port
    printfn "%A" wsServer.Address
    printfn "websocket server started"
