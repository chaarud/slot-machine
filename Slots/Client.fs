module Client

open Accounts.Account
open Nessos.FsPickler
open Listener

type Client (server : Server.Server, id) = 

    member self.SendRequest event = 
        sendRequest id event

    member self.Run initialFunds buyIn =
        self.SendRequest "Game Started"
        self.StartGame initialFunds buyIn
        |> self.GameLoop 0

    member self.GameLoop i account =
        // printfn "gameLoop iteration %A and %A" i account
        Async.RunSynchronously <| Async.Sleep 50
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
        printfn "Player has decided to stop playing"
        emptyAccount



