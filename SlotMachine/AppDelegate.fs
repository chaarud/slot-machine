namespace SlotMachine

open Client
open Metric
open Account

open System

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

        let generator i = async {
            let id = idAssigner.Next (1,1000000)
            let client = new Client (Id id)
            ignore <| client.Run 1000 10 
            } 

        for i in 1 .. 3 do
            generator i |> Async.Start

        Async.RunSynchronously <| Async.Sleep 5000
        System.Console.ReadLine () |> ignore    

//    ================================================

        true
