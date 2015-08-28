namespace Test

open UIKit

open Client
open Server

module Main = 
    [<EntryPoint>]
    let main args = 
        UIApplication.Main(args, null, "AppDelegate")

        let idAssigner = new System.Random ()
        Listener.startListening ()
        let server = new Server ()

        let generator i = async {
            let id = idAssigner.Next (1,1000000)
            let client = new Client (server, id)
            ignore <| client.Run 1000 10
            }

        let parallelClients = 
            List.init 1 generator
            |> Async.Parallel
            |> Async.RunSynchronously

        0
