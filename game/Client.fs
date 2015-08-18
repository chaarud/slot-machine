module Client

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

type Client () = 

    member self.GameLoop i rng server account =
        // printfn "gameLoop iteration %A and %A" i account
        ignore <| Async.Sleep 500
        match i >= 300 with
        | true -> self.GameOver ()
        | false -> 
            match leverPullable account with
            | true -> PullLever
            | false -> BuyMoney
            |> self.DoTransaction rng server account
            |> self.GameLoop (i+1) rng server

    member self.StartGame (server : Server.Server) money buyIn = 
        server.Initialize money buyIn

    member self.DoTransaction rng (server : Server.Server) account trx = 
        server.Transaction account rng trx

    member self.GameOver () = 
        printfn "Player has decided to stop playing"
        emptyAccount


[<EntryPoint>]
let main argv = 
    let server = new Server.Server ()
    let client = new Client ()
    let initialAccount = client.StartGame server 10000 10
    let rng = new System.Random ()

    let idNum = rng.Next (1, 100000000) 
    let id = idNum.ToString ()
    let result = 
        sendRequest id "Game Started"
        |> Async.RunSynchronously //Async.Start

    let finalAccount = client.GameLoop 0 rng server initialAccount
    0 // return an integer exit code

