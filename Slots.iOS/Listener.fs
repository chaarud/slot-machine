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

    let kinesisProvider = new KinesisProvider ()
    let kinesisClient = kinesisProvider.setup ()

    override self.OnMessage (e:MessageEventArgs) = 
        printfn "Listener should fire to Kinesis at this point"
        //ignore <| kinesisProvider.provide e.RawData kinesisClient
    
let startListening () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<KinesisService>("/KinesisService")
    wsServer.Start ()