open Client
open Server
open Listener
open Account
open fszmq 
open Nessos.FsPickler

//type DU = A | B | C
//type Tuple = Tuple of DU * int option
//type Tuple2 = Tuple2 of Tuple | Tuple2NA
//type Client () = 
//    let context = new Context ()
//    let client = Context.req context 
//    let pickler = FsPickler.CreateBinarySerializer ()
//    do Socket.connect client "tcp://localhost:5555"
//    member this.SendMessage message = 
//        pickler.Pickle message
//        |> Socket.send client 
//    member this.Go () = 
//        while true do
//            this.SendMessage (Tuple2 (Tuple (B, Some 3)))
//            System.Threading.Thread.Sleep ((new System.Random ()).Next (1, 1000) + 1)
//            Socket.recv client |> ignore
//
//type Server () = 
//    let context = new Context () 
//    let server = Context.rep context
//    do Socket.bind server "tcp://*:5555"
//    let pickler = FsPickler.CreateBinarySerializer ()
//    member this.Run () =
//        while true do 
//            let msg = Socket.recv server
//            let msg' = pickler.UnPickle<Tuple2> msg
//            printfn "I recieved %A" msg'
//            Socket.send server (pickler.Pickle "Next")

[<EntryPoint>]
let main argv = 

//    let server = new Server ()
//
//    async {server.Run ()} |> Async.Start
//
//    for i in 1 .. 3 do
//        async {new Client () |> fun x -> x.Go ()} |> Async.Start 
//    
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

//    let parallelClients = 
//        List.init 2 generator
//        |> List.iter
//        |> Async.Parallel
//        |> Async.RunSynchronously
//
    Async.RunSynchronously <| Async.Sleep 5000


    System.Console.ReadLine () |> ignore    

    0 // return an integer exit code

//    let flag = ref false
//    async {
//        while !flag do
//            // ...
//            event.Trigger
//            do! sleep
//        }