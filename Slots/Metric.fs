module Metric
    
open Account

type Metric = 
    | GameStarted
    | GameEnded
    | BuyMoneyMetric of Transaction*Account
    | PullLeverMetric of Transaction*Account
