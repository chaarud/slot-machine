module Account 

    type Id = Id of int
        
    type Transaction = 
        | PullLever
        | BuyMoney
        | EndGame

    type Account = {
        id : Id
        money : int Option
        buyIn : int Option } 

    let emptyAccount = { 
        id = Id 0
        money = None
        buyIn = None }

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
