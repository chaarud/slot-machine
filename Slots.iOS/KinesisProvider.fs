module KinesisProvider

open Amazon.Kinesis.Model
open Amazon.Kinesis
open Amazon

open Account
open Metric
open Nessos.FsPickler

type KinesisProvider () = 

    member self.setup () = 
        let awsAccessKeyId = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/access_key_id.config"
        let awsSecretAccessKey = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/secret_access_key.config"
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
