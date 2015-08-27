﻿module KinesisProvider

open FSharp.Configuration

open Amazon.Kinesis.Model
open Amazon.Kinesis
open Amazon

open Account
open Metric
open Nessos.FsPickler

type Settings = AppSettings<"app.config">

type KinesisProvider () = 

    member self.testRun () = 
        let r = new System.Random ()
        let client = self.setup ()
        while true do
            let tuple = (55555, GameEnded)
            let pickler = FsPickler.CreateBinarySerializer ()
            let pickle = pickler.Pickle tuple
//            let bytes : byte array = Array.create 20 0uy
//            r.NextBytes bytes
            ignore <| self.provide pickle client
            Async.RunSynchronously <| Async.Sleep 20

    member self.setup () = 
        let awsAccessKeyId = Settings.AwsAccessKeyId
        let awsSecretAccessKey = Settings.AwsSecretAccessKey
        let region = RegionEndpoint.USEast1
        new AmazonKinesisClient (awsAccessKeyId, awsSecretAccessKey, region)

    member self.provide (data : byte[]) (kinesisClient : AmazonKinesisClient) = 
        let putRecord = new PutRecordRequest ()
        putRecord.StreamName <- "SlotMachineProducerStream"
        putRecord.PartitionKey <- "blahblahblah"
        printfn "sent data" // %A" data
        putRecord.Data <- (new System.IO.MemoryStream (data))
        try
            kinesisClient.PutRecord(putRecord)
        with
            | _ -> failwith "Error sending record to Amazon Kinesis"
