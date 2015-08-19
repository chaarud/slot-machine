﻿namespace Accounts

module Account =

    type Account = {
        money : int Option
        buyIn : int Option } 

    let emptyAccount = { 
        money = None
        buyIn = None }

    type Transaction = 
        | PullLever
        | BuyMoney

    let buyIn account = 
        account.buyIn

    let money account = 
        account.money

    let leverPullable account = 
        match money account, buyIn account with
        | Some money, Some buyIn ->
            money > buyIn
        | _, _ -> 
            false