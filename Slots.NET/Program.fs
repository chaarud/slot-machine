open Client
open Server
open Listener
open Account

[<EntryPoint>]
let main argv = 
    let idAssigner = new System.Random ()
    Listener.startListening ()

    let server = new Server ()
    async {do server.Run ()} |> Async.Start

    let generator i = async {
        let id = idAssigner.Next (1,1000000)
        let client = new Client (Id id)
        ignore <| client.Run 1000 10 
        } 
    for i in 1 .. 3 do
        generator i |> Async.Start

    Async.RunSynchronously <| Async.Sleep 5000
    System.Console.ReadLine () |> ignore    
    0 // return an integer exit code
