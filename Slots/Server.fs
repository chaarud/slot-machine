module Server

open Account
open Metric

open Nessos.FsPickler
open WebSocketSharp

type Server () = 

    let random = new System.Random ()

    let pickler = FsPickler.CreateBinary ()

    let ws = new WebSocket("ws://localhost:55555/AmplitudeService")
    do 
        ws.OnOpen.Add (fun _ -> printfn "Slot Server's WebSocket opened")
        ws.OnClose.Add (fun _ -> printfn "Slot Server's WebSocket closed")
        ws.Connect ()

    member self.SendMetric id (metric : Metric) = 
        let tuple = id, metric
        let pickle = pickler.Pickle (tuple)
        ws.Send pickle //SendAsync vs Send

    member self.Initialize money buyIn = 
        match money > buyIn with
        | true -> { money = Some money; buyIn = Some buyIn}
        | false -> 
            printfn "Your account does not have enough money to start a new session"
            emptyAccount

    member self.Transaction account (id, transaction) =
        match transaction with
        | PullLever -> 
            let newAcct = self.DoPullLever account
            self.SendMetric id (PullLeverMetric transaction)
            newAcct
        | BuyMoney -> 
            let newAcct = self.DoBuyMoney account
            self.SendMetric id (BuyMoneyMetric transaction)
            newAcct

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
                    { money = Some <| payout+currentMoney; buyIn = Some buyIn}
                | false ->
                    { money = Some <| currentMoney-buyIn; buyIn = Some buyIn}
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
            { money = Some <| currentMoney + 1000; buyIn = Some buyIn}
        | _, _ ->
            printfn "Your account does not have money or buyIn information"
            emptyAccount
