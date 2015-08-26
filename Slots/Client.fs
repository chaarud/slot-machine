module Client

open Account
open Metric

open Nessos.FsPickler
open WebSocketSharp

type Client (server : Server.Server, id : int) = 

    let pickler = FsPickler.CreateBinary ()

    let platform = "Android"

    let ws = new WebSocket("ws://localhost:55555/KinesisService")
    do 
        ws.OnOpen.Add (fun _ -> printfn "Client's WebSocket opened")
        ws.OnClose.Add (fun _ -> printfn "Client's WebSocket closed")
        ws.Connect ()

    member self.SendMetric (metric : Metric) = 
        let tuple = id, metric
        let pickle = pickler.Pickle (tuple)
        ws.Send pickle //SendAsync vs Send

    member self.Run initialFunds buyIn =
        //self.SendMetric (GameStarted platform)
        self.StartGame initialFunds buyIn
        |> self.GameLoop 0

    member self.GameLoop i account =
        Async.RunSynchronously <| Async.Sleep 5
        match i >= 3000 with
        | true -> 
            self.GameOver id account
        | false -> 
            printfn "step %A" i
            match leverPullable account with
            | true -> 
                (id, PullLever)
            | false -> 
                (id, BuyMoney)
            |> self.DoTransaction account
            |> self.GameLoop (i+1)

    member self.StartGame money buyIn = 
        server.Initialize money buyIn

    member self.DoTransaction (account : Account) (id, trx) = 
        server.Transaction account (id,trx)

    member self.GameOver id account = 
        self.SendMetric GameEnded
        printfn "Player has decided to stop playing"
        let empty = self.DoTransaction account (id, EndGame)
        ws.Close ()
        empty

