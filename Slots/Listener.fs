module Listener

open Account
open Metric
open KinesisProvider

open FSharp.Data
open FSharp.Configuration
open Nessos.FsPickler
open WebSocketSharp
open WebSocketSharp.Server

open Amazon.Kinesis.Model
open Amazon.Kinesis

type Settings = AppSettings<"app.config">

type KinesisService () =
    inherit WebSocketBehavior ()

    let pickler = FsPickler.CreateBinary ()

    let kinesisProvider = new KinesisProvider ()
    let kinesisClient = kinesisProvider.setup ()

    override self.OnMessage (e:MessageEventArgs) = 
        ignore <| kinesisProvider.provide e.RawData kinesisClient

let startListening () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<KinesisService>("/KinesisService")
    wsServer.Start ()