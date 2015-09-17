module Client

open Account
open Metric

open UIKit

open System
open System.Text
open WebSocketSharp
open Publisher
type Client (server : Server.Server, id : Id) = 

    let oss = List.map OS ["iOS"; "Android"; "Tizen"]
    let devices = List.map Device ["iPhone 4"; "iPad"; "iPhone 5"]
    let countries = List.map Country ["USA"; "El Salvador"; "North Korea"]
    let address = "ws://localhost:55555/KinesisService"
    let rnd = new System.Random()
    let (Id id') = id
    let publisher = new ClientPublisher(address, id')
//    let os = OS (UIDevice.CurrentDevice.SystemName + " " + UIDevice.CurrentDevice.SystemVersion)
//    let device = Device UIDevice.CurrentDevice.Model
//    let country = Country (Foundation.NSLocale.CurrentLocale.GetCountryCodeDisplayName(Foundation.NSLocale.CurrentLocale.CountryCode))

    let os = List.nth oss (rnd.Next(oss.Length))
    let device = List.nth devices (rnd.Next(oss.Length))
    let country = List.nth countries (rnd.Next(oss.Length))



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
        publisher.Connect()
        publisher.SendMetric <| GameStarted (os, device, country)
        server.Initialize id money buyIn

    member self.DoTransaction (account : Account) (id, trx) = 
        server.Transaction account (id,trx)

    member self.GameOver id (account:Account) = 
        publisher.SendMetric <| GameEnded
        //how do we solve the problem of the websocket closing before the final event is sent off?
        Async.RunSynchronously <| Async.Sleep 10000
        printfn "Player has decided to stop playing"
        let empty = self.DoTransaction account (id, EndGame)
        publisher.Close ()
        empty

