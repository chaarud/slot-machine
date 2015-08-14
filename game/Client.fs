module Client

open Accounts.Account

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
    let initialAccount, server = startGame 10000 10
    let rng = new System.Random ()

    let playerId = rng.Next (1, 100000000)
    let playerDevice = "iPhone"

    let finalAccount = gameLoop 0 rng server initialAccount
    0 // return an integer exit code

