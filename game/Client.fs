module Client

open Accounts.Account
open Segment

let startGame money buyIn = 
    let server = new Server.Server ()
    let acct = server.Initialize money buyIn
    acct, server

let doTransaction rng (server : Server.Server) account trx = 
    server.Transaction account rng trx

let gameOver id = 
    printfn "Player has decided to stop playing"
    Analytics.Client.Track(id, "Logged Out")
    emptyAccount

let rec gameLoop id i rng server account =
    printfn "gameLoop iteration %A and %A" i account
    match i >= 300 with
    | true -> gameOver id 
    | false -> 
        match leverPullable account with
        | true -> 
            Analytics.Client.Track(id, "PullLever")
            PullLever
        | false -> 
            Analytics.Client.Track(id, "BuyMoney")
            BuyMoney
        |> doTransaction rng server account
        |> gameLoop id (i+1) rng server

[<EntryPoint>]
let main argv = 

    Analytics.Initialize("f6XVyYT24A2IHwNccCaXapA9FuZLnhXJ")

    let initialAccount, server = startGame 10000 10
    let rng = new System.Random ()

//    let idNum = rng.Next (1, 100000000)
//    let id = idNum.ToString ()
    let id = "Jeff"
//    let playerDevice = "iPhone"

    Analytics.Client.Identify(id, new Model.Traits ())
    printfn "%A" id
    Analytics.Client.Track(id, "Logged In")

    let finalAccount = gameLoop id 0 rng server initialAccount
    0 // return an integer exit code

