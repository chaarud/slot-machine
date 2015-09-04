module Listener

open Account
open Metric
open KinesisProvider

open FSharp.Data
open Nessos.FsPickler
open WebSocketSharp
open WebSocketSharp.Server

type KinesisService () =
    inherit WebSocketBehavior ()

    let pickler = FsPickler.CreateBinarySerializer ()

    override self.OnMessage (e:MessageEventArgs) = 
        ignore <| provide e.RawData
    
let startListening () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<KinesisService>("/KinesisService")
    wsServer.Start ()