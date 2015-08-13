module Client

open Accounts.Account

let startGame (money :int) (buyIn :int) :Account = 
    Server.initialize money buyIn

let pullLever rng account :Account = 
    Server.doLeverPull account rng

let gameOver () = 
    printfn "Player has decided to stop playing"
    emptyAccount

let rec gameLoop (i :int) (rng : System.Random) (account :Account) :Account =
    printfn "gameLoop iteration %A and %A" i account
    match i >= 300 with
    | true -> gameOver ()
    | false -> 
        match leverPullable account with
        | true -> 
            pullLever rng account
            |> gameLoop (i+1) rng
        | _ -> gameOver ()

[<EntryPoint>]
let main argv = 
    let initialAccount = startGame 10000 10
    let rng = new System.Random 4
    let finalAccount = gameLoop 0 rng initialAccount
    0 // return an integer exit code

