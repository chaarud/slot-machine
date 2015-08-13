module Client

open Accounts.Account

let pullLever account :Account = 
    Server.doLeverPull account

let gameOver () = 
    printfn "Player has decided to stop playing"
    emptyAccount

let rec gameLoop (i :int) (account :Account) :Account =
    printfn "gameLoop iteration %A and %A" i account
    match i >= 300 with
    | true -> gameOver ()
    | false -> 
        match leverPullable account with
        | true -> 
            pullLever account
            |> gameLoop (i+1) 
        | _ -> gameOver ()

let startGame (money :int) (buyIn :int) :Account = 
    Server.initialize money buyIn

[<EntryPoint>]
let main argv = 
    let initialAccount = startGame 10000 10
    let finalAccount = gameLoop 0 initialAccount
    0 // return an integer exit code

