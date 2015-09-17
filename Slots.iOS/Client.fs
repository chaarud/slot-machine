module Client

open Account
open Metric

open UIKit

open System
open System.Text
open Newtonsoft.Json
open Nessos.FsPickler
//open Nessos.FsPickler.Json
open WebSocketSharp

type Client (server : Server.Server, id : Id) = 

    let jsonSerializer = Nessos.FsPickler.Json.JsonSerializer()

    let oss = List.map OS ["iOS"; "Android"; "Tizen"]
    let devices = List.map Device ["iPhone 4"; "iPad"; "iPhone 5"]
    let countries = List.map Country ["USA"; "El Salvador"; "North Korea"]

    let rnd = new System.Random()

//    let os = OS (UIDevice.CurrentDevice.SystemName + " " + UIDevice.CurrentDevice.SystemVersion)
//    let device = Device UIDevice.CurrentDevice.Model
//    let country = Country (Foundation.NSLocale.CurrentLocale.GetCountryCodeDisplayName(Foundation.NSLocale.CurrentLocale.CountryCode))

    let os = List.nth oss (rnd.Next(oss.Length))
    let device = List.nth devices (rnd.Next(oss.Length))
    let country = List.nth countries (rnd.Next(oss.Length))

    let ws = new WebSocket("ws://localhost:55555/KinesisService")
    do 
        ws.OnOpen.Add (fun _ -> printfn "Client's WebSocket opened")
        ws.OnClose.Add (fun _ -> printfn "Client's WebSocket closed")
        ws.Connect ()

    member self.SendMetric (metric : Metric) = 
        printfn "serializing"
//        let mutable json = ""
        let json = jsonSerializer.Pickle(metric)
        printfn "serialized"
//        let unixTime() = 
//            let epoch = DateTime(1970, 1, 1) in (DateTime.Now - epoch).TotalMilliseconds |> int64    
//        let (Id(id')) = id
//        match metric with
//        | GameStarted (OS(os), Device(device), Country(country)) ->
//            printfn "getting json"
//            json <- 
//                sprintf """{"PlayerId": %d, "Time" : %d, "OS": "%s", "Device": "%s", "Country": "%s"}""" 
//                    id' (unixTime()) os device country
//            printfn "got json"
//        | GameEnded ->
//            json <-
//                sprintf """{"PlayerId": %d, "Time" : %d,}"""
//                    id' (unixTime()) 
//        let jsonBytes = Encoding.UTF8.GetBytes json
        ws.Send json

    member self.Run initialFunds buyIn =
        self.StartGame id initialFunds buyIn
        |> self.GameLoop 0

    member self.GameLoop i (account:Account) =
        Async.RunSynchronously <| Async.Sleep 5
        match i >= 10 with
        | true -> 
            self.GameOver id account
        | false -> 
            match leverPullable account with
            | true -> 
                (id, PullLever)
            | false -> 
                (id, BuyMoney)
            |> self.DoTransaction account
            |> self.GameLoop (i+1)

    member self.StartGame id money buyIn = 
        self.SendMetric <| GameStarted (os, device, country)
        server.Initialize id money buyIn

    member self.DoTransaction (account : Account) (id, trx) = 
        server.Transaction account (id,trx)

    member self.GameOver id (account:Account) = 
        self.SendMetric <| GameEnded
        //how do we solve the problem of the websocket closing before the final event is sent off?
        Async.RunSynchronously <| Async.Sleep 10000
        printfn "Player has decided to stop playing"
        let empty = self.DoTransaction account (id, EndGame)
        ws.Close ()
        empty

