﻿module Client

open Account
open Metric
open System
open Nessos.FsPickler
open WebSocketSharp
open FSharp.Data

type Client (id : Id) = 

    let pickler = FsPickler.CreateBinarySerializer ()

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

    let listenerWS = new WebSocket("ws://localhost:55555/KinesisService")
    do 
        listenerWS.OnOpen.Add (fun _ -> printfn "Client's WebSocket to Listener opened")
        listenerWS.OnClose.Add (fun _ -> printfn "Client's WebSocket to Listener closed")
        listenerWS.Connect ()

    member self.DoTransaction (account : Account) (id, trx) :Account = 
        let toPickle = (account, (id, trx))
        let pickle = pickler.Pickle<Account*(Id*Transaction)> (toPickle)
        let req = System.Convert.ToBase64String pickle
        let resp = self.Dispatch (req)
        let responseData = System.Convert.FromBase64String resp
        let account = pickler.UnPickle<Account> responseData
        account

    member self.Dispatch (req : string) = 
        let url = "http://localhost:5678/" + req
        Http.RequestString (url)

    member self.SendMetric (metric : Metric) = 
        let info = id, DateTime.UtcNow, metric
        let pickle = pickler.Pickle<Id*DateTime*Metric> (info)
        listenerWS.Send pickle //SendAsync vs Send

    member self.Run initialFunds buyIn =
        printfn "client running"
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
        self.DoTransaction {id = id; money = Some money; buyIn = Some buyIn} (id, Initialize)

    member self.GameOver id (account:Account) = 
        self.SendMetric <| GameEnded
        //how do we solve the problem of the websocket closing before the final event is sent off?
        Async.RunSynchronously <| Async.Sleep 10000
        printfn "Player has decided to stop playing"
        let empty = self.DoTransaction account (id, EndGame)
        listenerWS.Close ()
        empty

