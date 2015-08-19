module Client

open Accounts.Account
open Nessos.FsPickler
open WebSocketSharp
open Listener
open Metrics

type Client (server : Server.Server, id) = 

    let pickler = FsPickler.CreateBinary ()
    let ws = new WebSocket("ws://localhost:55555/AmplitudeService")
    //set ws.OnMessage? (only to deal with recieved messages)
    do 
        ws.OnOpen.Add (fun _ -> printfn "WebSocket opened")
        ws.Connect ()
    member self.SendRequest event = 
        let pickle = pickler.Pickle ((id,event))
        printfn "url to send to: %A" ws.Url
        printfn "sending pickle"
        ws.Send(pickle) //SendAsync?
        printfn "sent pickle"
        //Listener.sendRequest id event

    member self.Run initialFunds buyIn =
        self.SendRequest "Game Started"
        self.StartGame initialFunds buyIn
        |> self.GameLoop 0

    member self.GameLoop i account =
        //printfn "gameLoop iteration %A and %A" i account
        //Async.RunSynchronously <| Async.Sleep 50
        match i >= 300 with
        | true -> self.GameOver ()
        | false -> 
            match leverPullable account with
            | true -> PullLever
            | false -> BuyMoney
            |> self.DoTransaction account
            |> self.GameLoop (i+1)

    member self.StartGame money buyIn = 
        server.Initialize money buyIn

    member self.DoTransaction (account : Account) trx = 
        server.Transaction account trx

    member self.GameOver () = 
        ws.Close ()
        printfn "Player has decided to stop playing"
        emptyAccount

