module Server

open Accounts.Account

type Server () = 

    let serverEvent = new Event<Account> ()

    member self.Events () = serverEvent.Publish

    member self.Initialize money buyIn = 
        match money > buyIn with
        | true -> { money = Some money; buyIn = Some buyIn}
        | false -> 
            printfn "Your account does not have enough money to start a new session"
            emptyAccount

    member self.Transaction account random transaction =
        match transaction with
        | PullLever -> self.DoPullLever account random
        | BuyMoney -> self.DoBuyMoney account random
            
    member self.DoPullLever account (random :System.Random) =
        let r1 = random.Next(1,10)
        let r2 = random.Next(1,10)
        let buyIn = getBuyIn account
        let currentMoney = getMoney account
        match leverPullable account with
        | true ->
            match buyIn, currentMoney with
            | Some buyIn, Some currentMoney ->  
                let payout = buyIn * 10
                serverEvent.Trigger (account)
                match r1 = r2 with
                | true -> 
                    printfn "You won"
                    { money = Some <| payout+currentMoney; buyIn = Some buyIn}
                | false ->
                    { money = Some <| currentMoney-buyIn; buyIn = Some buyIn}
            | _, _ -> 
                printfn "Your account does not have money or buyIn information"
                emptyAccount
        | false -> 
            printfn "Your account does not have enough money to pull the lever"
            emptyAccount

    member self.DoBuyMoney account random =
        emptyAccount
