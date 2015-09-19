open SlotServer

[<EntryPoint>]
let main argv = 

    async {Listener.startListening ()} |> Async.Start

    System.Threading.Thread.Sleep 1000

    let server = new Server ()
    async {do server.Run ()} |> Async.Start

    ignore <| System.Console.ReadLine ()

    0 // return an integer exit code

