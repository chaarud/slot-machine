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
        let publisher = kinesisPublisher
        publisher.createPutRecordRequest (StreamName "Slots") (PartitionKey "partition0") e.RawData
        |> publisher.publish
        |> ignore
    
type SendJSON () = 
    inherit WebSocketBehavior ()
    override self.OnMessage (e: MessageEventArgs) = 
//        let deserialized = 
        printfn "receiving data : %A" e.Data//todo: jsonparsing function by looking at anadashboard

let startListening () = 
    let wsServer = new WebSocketServer("ws://localhost:55555")
    wsServer.AddWebSocketService<SendJSON>("/KinesisService")
    wsServer.Start ()