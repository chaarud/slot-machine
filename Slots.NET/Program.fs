﻿open Client
open Server
open Listener
open Account

[<EntryPoint>]
let main argv = 

    let idAssigner = new System.Random ()

    Listener.startListening ()

    let server = new Server ()
    async {do server.Run ()} |> Async.Start
//    server.Run ()
    printfn "Server running"

//    let generator i = async {
//        let id = idAssigner.Next (1,1000000)
//        let client = new Client (Id id)
//        ignore <| client.Run 1000 10 
//        }

//    let parallelClients = 
//        printfn "spinning up client"
//        List.init 1 generator
//        |> Async.Parallel
//        |> Async.RunSynchronously

    let client = new Client (Id 6)
    printfn "created client"
    ignore <| client.Run 1000 10

//    let clientProcess = async {
//        printfn "client async"
//        let client = new Client (Id 5)
//        ignore <| client.Run 1000 10 }
//    printfn "feeding client async to async.start"
//    clientProcess |> Async.Start
//    printfn "done starting client async"

    Async.RunSynchronously <| Async.Sleep 5000
    printfn "process over"    

    0 // return an integer exit code

