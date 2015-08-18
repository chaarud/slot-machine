module Client

open Accounts.Account
//open System.Net
//open System.Text
open FSharp.Data



let sendRequest () = //id event = 
    let url = "https://api.amplitude.com/httpapi"
    let api_key = "9d7c111da3e4eaa3e6c2a36f90283424"
    let event = """[{"user_id":"Bob","event_type":"loginEvent"}]"""
    let requestBody = FormValues[("api_key", api_key ); ("event", event)]
    Http.RequestString(url, body = requestBody)

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
    printfn "gameLoop iteration %A and %A" i account
    match i >= 300 with
    | true -> gameOver ()
    | false -> 
        match leverPullable account with
        | true -> PullLever
        | false -> BuyMoney
        |> doTransaction rng server account
        |> gameLoop (i+1) rng server

[<EntryPoint>]
let main argv = 
//    let initialAccount, server = startGame 10000 10
//    let rng = new System.Random ()

    let x = sendRequest ()
    printfn "%A" x

//    let idNum = rng.Next (1, 100000000) 
//    let id = idNum.ToString ()
//    let playerDevice = "iPhone"

//    let finalAccount = gameLoop 0 rng server initialAccount
    0 // return an integer exit code

