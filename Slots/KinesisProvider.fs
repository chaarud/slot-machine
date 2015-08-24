﻿module KinesisProvider

open FSharp.Configuration

open Amazon.Kinesis.Model
open Amazon.Kinesis
open Amazon

type Settings = AppSettings<"app.config">

type KinesisProvider () = 

    member self.testRun () = 
        let r = new System.Random ()
        let client = self.setup ()
        while true do
            let bytes : byte array = Array.create 200 0uy
            r.NextBytes bytes
            ignore <| self.provide bytes client
            Async.RunSynchronously <| Async.Sleep 1000

    member self.setup () = 

        let awsAccessKeyId = Settings.AwsAccessKeyId
        let awsSecretAccessKey = Settings.AwsSecretAccessKey
        let awsSessionToken = Settings.AwsSessionToken

        let region = RegionEndpoint.USEast1

        let amazonKinesisClient = new AmazonKinesisClient (awsAccessKeyId, awsSecretAccessKey, awsSessionToken, region)

        amazonKinesisClient

    member self.provide (data:byte[]) (kinesisClient:AmazonKinesisClient) = 
        let putRecord = new PutRecordRequest ()
        putRecord.StreamName <- "SlotMachineKinesisStream"
        putRecord.PartitionKey <- "blahblahblah"
        putRecord.Data <- (new System.IO.MemoryStream (data))
        try
            kinesisClient.PutRecord(putRecord)
        with
            | _ -> failwith "Error sending record to Amazon Kinesis"
