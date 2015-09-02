namespace SlotMachine

open AmplitudeSdk

open System

open Client
open Server
open Metric

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

        let generator i = async {
            let id = idAssigner.Next (1,1000000)
            let client = new Client (server, id)
            ignore <| client.Run 1000 10
            }

        let parallelClients = 
            List.init 3 generator
            |> Async.Parallel
            |> Async.RunSynchronously

        Async.RunSynchronously <| Async.Sleep 5000

//    ================================================

        true
