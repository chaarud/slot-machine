open SlotServer

[<EntryPoint>]
let main argv = 

    let server = new Server ()
    async {do server.Run ()} |> Async.Start

    ignore <| System.Console.ReadLine ()

    0 // return an integer exit code

