namespace Accounts

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

    let getBuyIn account = 
        account.buyIn

    let getMoney account = 
        account.money

    let leverPullable account = 
        match getMoney account, getBuyIn account with
        | Some m, Some b ->
            m > b
        | _, _ -> 
            false
