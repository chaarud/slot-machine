module Metric
    
open Account

type OS = OS of string

type Device = Device of string

type Country = Country of string

type ClientMetric = 
    | GameStarted of OS * Device * Country
    | GameEnded

type ServerMetric = 
    | TransactionMetric of Transaction * Account