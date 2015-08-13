module Client

open Accounts.Account

let startGame (money :int) (buyIn :int) = 
    let server = new Server.Server ()
    let acct = server.Initialize money buyIn
    acct, server

let pullLever rng (server:Server.Server) account :Account = 
    server.DoLeverPull account rng

let gameOver () = 
    printfn "Player has decided to stop playing"
    emptyAccount

let rec gameLoop (i :int) (rng : System.Random) (server : Server.Server) (account :Account) :Account =
    printfn "gameLoop iteration %A and %A" i account
    match i >= 300 with
    | true -> gameOver ()
    | false -> 
        match leverPullable account with
        | true -> 
            pullLever rng server account
            |> gameLoop (i+1) rng server
        | _ -> gameOver ()

[<EntryPoint>]
let main argv = 
    let initialAccount, server = startGame 10000 10
    let rng = new System.Random 4
    let finalAccount = gameLoop 0 rng server initialAccount
    0 // return an integer exit code

