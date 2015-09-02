module Account 

    type Transaction = 
        | PullLever
        | BuyMoney
        | EndGame

    type Account = {
        money : int Option
        buyIn : int Option } 

    let emptyAccount = { 
        money = None
        buyIn = None }

    let buyIn account = 
        account.buyIn

    let money account = 
        account.money

    let leverPullable account = 
        | Some money, Some buyIn ->
            money > buyIn
        | _, _ -> 
            false
