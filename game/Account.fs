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

    let getBuyIn (account:Account) :int Option = 
        account.buyIn

    let getMoney (account:Account) :int Option = 
        account.money

    let leverPullable account = 
        getMoney account > getBuyIn account
