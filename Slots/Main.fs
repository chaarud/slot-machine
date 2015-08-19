open Client
open Server
open Listener

[<EntryPoint>]
let main argv = 
    let idAssigner = new System.Random ()
    let server = new Server ()
    Listener.startListener ()

    let generator i = async {
        let idNum = idAssigner.Next (1,1000000)
        let id = idNum.ToString ()
        let client = new Client (server, id)
        ignore <| client.Run 1000 10
        }

    let parallelClients = 
        List.init 1 generator
        |> Async.Parallel
        |> Async.RunSynchronously

    let s = System.Console.ReadLine ()

    0 // return an integer exit code
