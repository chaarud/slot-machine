namespace SlotMachine

open System

open Client
open Server
open Metric
open Account
open KinesisProvider

open System.Security
open System.Text

open UIKit
open Foundation

[<Register ("AppDelegate")>]
type AppDelegate () =
    inherit UIApplicationDelegate ()

    override val Window = null with get, set

    // This method is invoked when the application is ready to run.
    override this.FinishedLaunching (app, options) =

//        let api_key = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/SlotsApp.iOS/api_key.config"
//        let amp = Amplitude.Instance
//        match amp with
//        | null -> printfn "why is Amplitude.Instance null?: %A" amp
//        | _ -> amp.InitializeApiKey (api_key)
//        Amplitude.Instance.LogEvent ("iOS test event")
//        printfn "done for now"

//    ================================================

        let idAssigner = new System.Random ()

        Listener.startListening ()

        let server = new Server ()
        //async {do server.Run ()} |> Async.Start
        server.Run ()
        printfn "Server running"

//        let generator i = async {
//            let id = idAssigner.Next (1,1000000)
//            let client = new Client (Id id)
//            ignore <| client.Run 1000 10 
//            }
//
//        let parallelClients = 
//            printfn "spinning up client"
//            List.init 1 generator
//            |> Async.Parallel
//            |> Async.RunSynchronously

//        let client = new Client (Id 6)
//        printfn "created client"
//        ignore <| client.Run 1000 10

//        let clientProcess = async {
//            printfn "client async"
//            let client = new Client (Id 5)
//            ignore <| client.Run 1000 10 }
//        printfn "feeding client async to async.start"
//        clientProcess |> Async.Start
//        printfn "done starting client async"

        Async.RunSynchronously <| Async.Sleep 5000
        printfn "process over"

//    ================================================

        true
