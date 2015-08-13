module Server

open Accounts.Account

type Server () = 

    let serverEvent = new Event<Account> ()

    member self.Events () = serverEvent.Publish

    member self.Initialize money buyIn = 
        match money > buyIn with
        | true -> Account [Money(money); BuyIn(buyIn)]
        | false -> 
            printfn "Your account does not have enough money to start a new session"
            emptyAccount

    member self.DoLeverPull account (random :System.Random) =
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
                    Account [Money(payout+currentMoney); BuyIn(buyIn)]
                | false ->
                    Account [Money(currentMoney-buyIn); BuyIn(buyIn)]
            | _, _ -> 
                printfn "Your account does not have money or buyIn information"
                emptyAccount
        | false -> 
            printfn "Your account does not have enough money to pull the lever"
            emptyAccount

