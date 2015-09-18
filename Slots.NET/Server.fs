module Server

open Account
open Metric
open Nessos.FsPickler
open WebSocketSharp
open Suave
open Suave.Web
open Suave.Http
open Suave.Types
open Suave.Http.Successful
open Suave.Http.Applicatives
open System
open System.IO

type Server () = 
    let random = new System.Random ()
    let pickler = FsPickler.CreateBinarySerializer ()

    let listenerWS = new WebSocket("ws://localhost:55555/KinesisService")
    do 
        listenerWS.OnOpen.Add (fun _ -> printfn "Slot Server's WebSocket opened")
        listenerWS.OnClose.Add (fun _ -> printfn "Slot Server's WebSocket closed")
        listenerWS.Connect ()

    let serverConfig =
        let port = 5678
        { defaultConfig with
            homeFolder = Some __SOURCE_DIRECTORY__
            logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
            bindings = [ Types.HttpBinding.mk' Types.HTTP "127.0.0.1" port ] }

    member self.Run () =
        GET >>= choose [ pathScan "/%s" (fun req -> OK (self.Dispatch(req)))]                    
        |> startWebServer serverConfig

    member self.Dispatch (req : string) = 
        let request = System.Convert.FromBase64String req
        let (account, (id, trx)) = pickler.UnPickle<Account*(Id*Transaction)> request
        let newAcct = self.Transaction account (id, trx)
        Async.RunSynchronously <| Async.Sleep 1000
        let pickle = pickler.Pickle<Account> newAcct
        System.Convert.ToBase64String pickle

    member self.SendMetric id (metric : Metric) = 
        let tuple = id, metric
        let pickle = pickler.Pickle (tuple)
        listenerWS.Send pickle //SendAsync vs Send

    member self.DoInitialize id account = 
        let m = money account
        let b = buyIn account
        match m, b with
        | Some money, Some buyIn ->
            match money > buyIn with
            | true -> {id = id; money = Some money; buyIn = Some buyIn}
            | false -> 
                printfn "Your account does not have enough money to start a new session"
                emptyAccount
        | _ -> emptyAccount

    member self.Transaction account (id, transaction) =
        match transaction with
        | Initialize ->
            let newAcct = self.DoInitialize id account
            newAcct
        | PullLever -> 
            let newAcct = self.DoPullLever account
            //self.SendMetric id (PullLeverMetric (transaction, newAcct))
            newAcct
        | BuyMoney -> 
            let newAcct = self.DoBuyMoney account
            //self.SendMetric id (BuyMoneyMetric (transaction, newAcct))
            newAcct
        | EndGame ->
            printfn "Server reports that a client %A ended a game" id
            emptyAccount

    member self.DoPullLever account =
        let r1 = random.Next(1,10)
        let r2 = random.Next(1,10)
        let buyIn = buyIn account
        let currentMoney = money account
        match leverPullable account with
        | true ->
            match buyIn, currentMoney with
            | Some buyIn, Some currentMoney ->  
                let payout = buyIn * 10
                match r1 = r2 with
                | true -> 
                    { id = account.id; money = Some <| payout+currentMoney; buyIn = Some buyIn}
                | false ->
                    { id = account.id; money = Some <| currentMoney-buyIn; buyIn = Some buyIn}
            | _, _ -> 
                printfn "Your account does not have money or buyIn information"
                emptyAccount
        | false -> 
            printfn "Your account does not have enough money to pull the lever"
            emptyAccount

    member self.DoBuyMoney account =
        let buyIn = buyIn account
        let currentMoney = money account
        match buyIn, currentMoney with
        | Some buyIn, Some currentMoney ->
            { id = account.id; money = Some <| currentMoney + 1000; buyIn = Some buyIn}
        | _, _ ->
            printfn "Your account does not have money or buyIn information"
            emptyAccount
