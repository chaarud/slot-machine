﻿module Server

open Accounts.Account

type Server () = 

    let serverEvent = new Event<Account> ()
    let invalidFieldEvent = new Event<unit> ()

    member self.ServerEvents () = serverEvent.Publish
    member self.InvalidFieldEvents () = invalidFieldEvent.Publish

    member self.random = new System.Random ()

    member self.Initialize money buyIn = 
        match money > buyIn with
        | true -> { money = Some money; buyIn = Some buyIn}
        | false -> 
            printfn "Your account does not have enough money to start a new session"
            emptyAccount

    member self.Transaction account transaction =
        match transaction with
        | PullLever -> self.DoPullLever account
        | BuyMoney -> self.DoBuyMoney account
            
    member self.DoPullLever account =
        let r1 = self.random.Next(1,10)
        let r2 = self.random.Next(1,10)
        let buyIn = buyIn account
        let currentMoney = money account
        match leverPullable account with
        | true ->
            match buyIn, currentMoney with
            | Some buyIn, Some currentMoney ->  
                let payout = buyIn * 10
                serverEvent.Trigger (account)
                match r1 = r2 with
                | true -> 
                    // printfn "You won"
                    { money = Some <| payout+currentMoney; buyIn = Some buyIn}
                | false ->
                    { money = Some <| currentMoney-buyIn; buyIn = Some buyIn}
            | _, _ -> 
                printfn "Your account does not have money or buyIn information"
                invalidFieldEvent.Trigger ()
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
            invalidFieldEvent.Trigger ()
            emptyAccount