﻿module Client

open Accounts.Account
open FSharp.Data
open FSharp.Configuration

type Settings = AppSettings<"app.config">

let sendRequest id event = async {
    let url = "https://api.amplitude.com/httpapi"
    let api_key = Settings.ApiKey
    let event = "[{\"user_id\":\"" + id + "\",\"event_type\":\"" + event + "\"}]"
    let requestBody = FormValues[("api_key", api_key); ("event", event)]
    let status = Http.RequestString(url, body = requestBody) 
    printfn "Async HTTP request status: %A" status
    ignore status
    }

type Client (server : Server.Server, id) = 

    member self.Run initialFunds buyIn =
        let result = 
            sendRequest id "Game Started"
            |> Async.RunSynchronously //Async.Start

        self.StartGame initialFunds buyIn
        |> self.GameLoop 0

    member self.GameLoop i account =
        // printfn "gameLoop iteration %A and %A" i account
        ignore <| Async.Sleep 500
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

[<EntryPoint>]
let main argv = 
    let idAssigner = new System.Random ()
    let server = new Server.Server ()

    let idNum = idAssigner.Next (1,1000000)
    let id = idNum.ToString ()
    let client = new Client (server, id)
    let finalAccount = client.Run 1000 10

    let s = System.Console.ReadLine ()

    0 // return an integer exit code

