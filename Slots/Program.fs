module Main

open Client
open Server
open Listener

open KinesisProvider

open Nessos.FsPickler

[<EntryPoint>]
let main argv = 

//    let idAssigner = new System.Random ()
//    Listener.startListening ()
//    let server = new Server ()
//
//    let generator i = async {
//        let id = idAssigner.Next (1,1000000)
//        let client = new Client (server, id)
//        ignore <| client.Run 1000 10
//        }
//
//    let parallelClients = 
//        List.init 1 generator
//        |> Async.Parallel
//        |> Async.RunSynchronously
//
//    let s = System.Console.ReadLine ()

    printfn "making Provider object"
    let provider = new KinesisProvider ()
    printfn "entering testRun ()"
    provider.testRun ()        

    0 // return an integer exit code
