module Server

open Account
open Metric
open Newtonsoft.Json
open Nessos.FsPickler
open WebSocketSharp
open System.Text
open Publisher

type Server () = 

    let random = new System.Random ()

    let pickler = FsPickler.CreateBinarySerializer ()
    let address = "ws://localhost:55555/KinesisService"
    let publisher = new ServerPublisher(address)

    member self.Initialize id money buyIn = 
        match money > buyIn with
        | true -> { id = id; money = Some money; buyIn = Some buyIn}
        | false -> 
            printfn "Your account does not have enough money to start a new session"
            emptyAccount

    member self.Transaction account (id, transaction) =
        match transaction with
        | PullLever -> 
            let newAcct = self.DoPullLever account
            //publisher.SendMetric id (PullLeverMetric (transaction, newAcct))
            newAcct
        | BuyMoney -> 
            let newAcct = self.DoBuyMoney account
            //publisher.SendMetric id (BuyMoneyMetric (transaction, newAcct))
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
