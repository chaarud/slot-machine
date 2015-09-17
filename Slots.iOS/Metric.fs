module Metric 
open Account

type OS = OS of string

type Device = Device of string

type Country = Country of string

type Metric = 
    | GameStarted of OS*Device*Country
    | GameEnded
//    | BuyMoneyMetric of Transaction*Account
//    | PullLeverMetric of Transaction*Account
