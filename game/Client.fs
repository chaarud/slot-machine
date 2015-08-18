module Client

open Accounts.Account
open FSharp.Data
open FSharp.Configuration

type Settings = AppSettings<"app.config">

let sendRequest id event = async {
    printfn "1"
    let url = "https://api.amplitude.com/httpapi"
    printfn "1"
    let api_key = Settings.ApiKey
    printfn "1"
    let event = "[{\"user_id\":\"" + id + "\",\"event_type\":\"" + event + "\"}]"
    printfn "1"
    let requestBody = FormValues[("api_key", api_key); ("event", event)]
    printfn "about to send HTTP request"
    let status = Http.RequestString(url, body = requestBody) 
    printfn "Async HTTP request status: %A" status
    ignore status
    }

let startGame money buyIn = 
    let server = new Server.Server ()
    let acct = server.Initialize money buyIn
    acct, server

let doTransaction rng (server : Server.Server) account trx = 
    server.Transaction account rng trx

let gameOver () = 
    printfn "Player has decided to stop playing"
    emptyAccount

let rec gameLoop i rng server account =
//    printfn "gameLoop iteration %A and %A" i account
    match i >= 30000 with
    | true -> gameOver ()
    | false -> 
        match leverPullable account with
        | true -> PullLever
        | false -> BuyMoney
        |> doTransaction rng server account
        |> gameLoop (i+1) rng server

[<EntryPoint>]
let main argv = 
    let initialAccount, server = startGame 10000 10
    let rng = new System.Random ()

    let idNum = rng.Next (1, 100000000) 
    let id = idNum.ToString ()

    let result = 
        sendRequest id "Game Started"
        |> Async.Start //Async.Start

    let finalAccount = gameLoop 0 rng server initialAccount
    0 // return an integer exit code

