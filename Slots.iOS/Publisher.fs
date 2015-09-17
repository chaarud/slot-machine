module Publisher

open Metric
open System
open System.Text
open Newtonsoft.Json
open WebSocketSharp

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
    sprintf """{"playerId" : %d, "eventProperties": [%s], "userProperties": [%s]}"""
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
        | _ -> printfn "client can't do this"; ()
        let jsonBytes = Encoding.UTF8.GetBytes json
        publisher.Send json

    member self.Connect () = publisher.Connect()
    member self.Close() = publisher.Close()

type ServerPublisher(address) = 
    let publisher = Publisher(address)
    do publisher.Connect()

    member self.SendMetric(id, metric) = 
        let mutable json = ""
        match metric with
        | PullLeverMetric(transaction, account) -> ()
        | BuyMoneyMetric(transaction, account) -> ()
        | _ -> printfn "server can't do this"; ()
        json