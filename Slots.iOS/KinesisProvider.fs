module KinesisProvider

//open Amazon.Kinesis.Model
//open Amazon.Kinesis
//open Amazon

open Account
open Metric
open Nessos.FsPickler

type KinesisProvider () = 

//    member self.testRun () = 
//        let r = new System.Random ()
//        let client = self.setup ()
//        while true do
//            let tuple = (55555, GameEnded)
//            let pickler = FsPickler.CreateBinarySerializer ()
//            let pickle = pickler.Pickle tuple
////            let bytes : byte array = Array.create 20 0uy
////            r.NextBytes bytes
//            ignore <| self.provide pickle client
//            Async.RunSynchronously <| Async.Sleep 20

    member self.setup () = 
        let awsAccessKeyId = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/access_key_id.config"
        let awsSecretAccessKey = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/secret_access_key.config"
        1
//        RegionEndpoint.USEast1
//        let region = RegionEndpoint.USEast1
//        new AmazonKinesisClient (awsAccessKeyId, awsSecretAccessKey, region)
//
//    member self.provide (data : byte[]) (kinesisClient : AmazonKinesisClient) = 
//        let putRecord = new PutRecordRequest ()
//        putRecord.StreamName <- "SlotMachineProducerStream"
//        putRecord.PartitionKey <- "blahblahblah"
//        printfn "sent data" // %A" data
//        putRecord.Data <- (new System.IO.MemoryStream (data))
//        try
//            printfn "this is where we would put record" //kinesisClient.PutRecord(putRecord)
//        with
//            | _ -> failwith "Error sending record to Amazon Kinesis"
