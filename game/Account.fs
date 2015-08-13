namespace Accounts

module Account =

    type AccountItem = 
        | Money of int
        | BuyIn of int

    type Account = Account of AccountItem list

    type Transaction = 
        | PullLever of Account

    let emptyAccount = Account []

    let isBuyIn = function
        | BuyIn _ -> true
        | _ -> false

    let getBuyIn (Account(items)) :int Option = 
        match List.tryFind isBuyIn items with
        | Some (BuyIn(b)) -> Some b
        | _ -> None

    let isMoney = function
        | Money _ -> true
        | _ -> false

    let getMoney (Account(items)) :int Option = 
        match List.tryFind isMoney items with
        | Some (Money(m)) -> Some m
        | _ -> None

    let leverPullable account = 
        getMoney account > getBuyIn account
