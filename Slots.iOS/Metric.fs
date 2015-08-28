module Metric
    
open Account

type Metric = 
    | GameStarted of string
    | GameEnded
    | BuyMoneyMetric of Transaction*Account
    | PullLeverMetric of Transaction*Account
