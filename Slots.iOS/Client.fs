module Client

open Account
open Metric

open UIKit

open Nessos.FsPickler
open WebSocketSharp

type Client (server : Server.Server, id : Id) = 

    let pickler = FsPickler.CreateBinarySerializer ()

    let os = OS (UIDevice.CurrentDevice.SystemName + " " + UIDevice.CurrentDevice.SystemVersion)
    let device = Device UIDevice.CurrentDevice.Model

    let ws = new WebSocket("ws://localhost:55555/KinesisService")
    do 
        ws.OnOpen.Add (fun _ -> printfn "Client's WebSocket opened")
        ws.OnClose.Add (fun _ -> printfn "Client's WebSocket closed")
        ws.Connect ()

    member self.SendMetric (metric : Metric) = 
        let tuple = id, metric
        let pickle = pickler.Pickle<Id*Metric> (tuple)
        printfn "about to send"
        ws.Send pickle //SendAsync vs Send

    member self.Run initialFunds buyIn =
        self.StartGame id initialFunds buyIn
        |> self.GameLoop 0

    member self.GameLoop i (account:Account) =
        Async.RunSynchronously <| Async.Sleep 5
        match i >= 10 with
        | true -> 
            self.GameOver id account
        | false -> 
            match leverPullable account with
            | true -> 
                (id, PullLever)
            | false -> 
                (id, BuyMoney)
            |> self.DoTransaction account
            |> self.GameLoop (i+1)

    member self.StartGame id money buyIn = 
        self.SendMetric <| GameStarted (os, device)
        server.Initialize id money buyIn

    member self.DoTransaction (account : Account) (id, trx) = 
        server.Transaction account (id,trx)

    member self.GameOver id (account:Account) = 
        self.SendMetric <| GameEnded
        Async.RunSynchronously <| Async.Sleep 10000
        printfn "Player has decided to stop playing"
        let empty = self.DoTransaction account (id, EndGame)
        ws.Close ()
        empty

