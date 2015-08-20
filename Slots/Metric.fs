module Metric
    
open Account

type Metric = 
    | GameStarted
    | GameEnded
    | BuyMoneyMetric of Transaction
    | PullLeverMetric of Transaction
