module Publisher

open Metric
open System
open System.Text
open WebSocketSharp
open Account

let makeJsonList (properties:(string * string) List) = 
    match properties with
    | [] -> ""
    | _ ->
        let json = 
            properties
            |> List.fold(fun acc (name, value) ->
                acc + "\"" + name + "\"" + ":" + "\"" + value + "\""+ ", " 
            ) ""
        json.Substring(0, json.Length-2)


let makeJson playerId userProperties eventProperties = 
    let jsonUserProperties = makeJsonList userProperties
    let jsonEventProperties = makeJsonList eventProperties
    sprintf """{"playerId" : %d, "eventProperties": {%s}, "userProperties": {%s}}"""
        playerId jsonEventProperties jsonUserProperties

type Publisher(address) = 
    let ws = new WebSocket(address)

    member self.unixTime() = 
        let epoch = DateTime(1970, 1, 1) in (DateTime.Now - epoch).TotalMilliseconds |> string 

    member self.Send(data:string) = 
        ws.Send data
    
    member self.Connect() = 
        ws.OnOpen.Add (fun _ -> printfn "Client's WebSocket opened")
        ws.OnClose.Add(fun _ -> printfn "Client's WebSocket closed")
        ws.Connect()

    member self.Close() = 
        ws.Close()
 
type ClientPublisher(address, id') = 
    let publisher =  Publisher(address)

    member self.SendMetric(metric) = 
        let mutable json = ""
        match metric with
        | GameStarted (OS(os), Device(device), Country(country)) ->
            let userProperties  = [("OS",os); ("Device",device) ; ("Country", country)]
            let eventProperties = [("Type", "GameStarted"); ("Time", (publisher.unixTime()))]
            json <- makeJson id' userProperties eventProperties

        | GameEnded ->
            let userProperties = []
            let eventProperties = [("Type", "GameEnded"); ("Time", (publisher.unixTime()))]
            json <- makeJson id' userProperties eventProperties

        let jsonBytes = Encoding.UTF8.GetBytes json
        publisher.Send json

    member self.Connect () = publisher.Connect()
    member self.Close() = publisher.Close()

type ServerPublisher(address) = 
    let publisher = Publisher(address)
    do publisher.Connect()

    member self.SendMetric(id, metric:ServerMetric) = 
        let mutable json = ""
        let mutable eventProperties = []
        let (TransactionMetric(transaction, account)) = metric
        let (Id accountId) = account.id

        let buyIn = 
            match account.buyIn with
            | None -> 0
            | Some buy -> buy

        let money = 
            match account.money with
            | None -> 0
            | Some mons -> mons

        match transaction with
        | PullLever -> 
            eventProperties <- [("Type", "PullLever"); ("Time", (publisher.unixTime()))]
        | BuyMoney -> 
            eventProperties <- [("Type", "BuyMoney"); ("Time", (publisher.unixTime()))]
        | EndGame -> 
            eventProperties <- [("Type", "BuyMoney"); ("Time", (publisher.unixTime()))]
        | _ -> failwith "Case not covered" //TODO
        let userAccProperties = [("AccountId", string <| accountId) 
                                 ("Money", string <| money)
                                 ("BuyIn", string <| buyIn)]
        let json = makeJson id userAccProperties eventProperties
        let jsonBytes = Encoding.UTF8.GetBytes json
        publisher.Send json
    
    member self.Connect () = publisher.Connect()
    member self.Close() = publisher.Close()