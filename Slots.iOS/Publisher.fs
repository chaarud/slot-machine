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

type Publisher(address, id') = 
    let ws = new WebSocket(address)

    let makeJson playerId userProperties eventProperties = 
        let jsonUserProperties = makeJsonList userProperties
        let jsonEventProperties = makeJsonList eventProperties
        sprintf """{"playerId" : %d, "eventProperties": [%s], "userProperties": [%s]}"""
            playerId jsonEventProperties jsonUserProperties


    member self.Connect() = 
        ws.OnOpen.Add (fun _ -> printfn "Client's WebSocket opened")
        ws.OnClose.Add(fun _ -> printfn "Client's WebSocket closed")
        ws.Connect()

    member self.SendMetric(metric) = 
        let mutable json = ""
        printfn "serialized"
        let unixTime() = 
            let epoch = DateTime(1970, 1, 1) in (DateTime.Now - epoch).TotalMilliseconds |> string 
        match metric with
        | GameStarted (OS(os), Device(device), Country(country)) ->
            let userProperties  = [("OS",os); ("Device",device) ; ("Country", country)]
            let eventProperties = [("Type", "GameStarted"); ("Time", (unixTime()))]
            json <- makeJson id' userProperties eventProperties

        | GameEnded ->
            let userProperties = []
            let eventProperties = [("Type", "GameEnded"); ("Time", (unixTime()))]
            json <- makeJson id' userProperties eventProperties
        let jsonBytes = Encoding.UTF8.GetBytes json
        ws.Send json

    member self.Close() = 
        ws.Close()
 