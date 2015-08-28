namespace SlotMachine

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

        true
