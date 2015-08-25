module KinesisConsumer

open FSharp.Configuration
open Nessos.FsPickler

open Metric

open Amazon.Kinesis.Model
open Amazon.Kinesis
open Amazon

type Settings = AppSettings<"app.config">

type KinesisConsumer () = 

    member self.createClient () = 
        let awsAccessKeyId = Settings.AwsAccessKeyId
        let awsSecretAccessKey = Settings.AwsSecretAccessKey
        let region = RegionEndpoint.USEast1
        new AmazonKinesisClient (awsAccessKeyId, awsSecretAccessKey, region)

    member self.getShardId (client : AmazonKinesisClient) = 
        let describeStreamRequest = new DescribeStreamRequest ()
        describeStreamRequest.StreamName <- "SlotMachineKinesisStream"
//        might need a while loop because describestreams may not get all the shards
//        http://docs.aws.amazon.com/kinesis/latest/dev/kinesis-using-sdk-java-retrieve-shards.html
//        Alternatively, use KCL
        let describeStreamResult : DescribeStreamResponse = client.DescribeStream (describeStreamRequest)
        let shards = describeStreamResult.StreamDescription.Shards.ToArray ()
        printfn "shards array %A" shards
        match shards.Length > 0 with
        | true -> 
            shards.[0].ShardId
        | _ -> 
            printfn "zero length array, should not be reached"
            let s = new Shard ()
            s.ShardId

    member self.getShardIterator (client : AmazonKinesisClient, shardId) = 
        let shardIteratorRequest = new GetShardIteratorRequest ()
        shardIteratorRequest.StreamName <- "SlotMachineKinesisStream"
        shardIteratorRequest.ShardId <- shardId
        shardIteratorRequest.ShardIteratorType <- new ShardIteratorType ("TRIM_HORIZON")
        let shardIteratorResponse = client.GetShardIterator (shardIteratorRequest)
        let mutable shardIterator = shardIteratorResponse.ShardIterator
        shardIterator

    member self.getRecords (client : AmazonKinesisClient, shardIterator) = 
        let mutable shardIterator = shardIterator
        while true do
            let getRecordsRequest = new GetRecordsRequest ()
            getRecordsRequest.ShardIterator <- shardIterator
            getRecordsRequest.Limit <- 25
            let getRecordsResponse : GetRecordsResponse = client.GetRecords (getRecordsRequest)
            let records = getRecordsResponse.Records.ToArray ()
            match records.Length > 0 with
            | true -> 
                let firstRecord = records.[0]
                shardIterator <- getRecordsResponse.NextShardIterator
                let dataStream : System.IO.MemoryStream = firstRecord.Data
                let data = dataStream.ToArray ()
                let pickler = FsPickler.CreateBinary ()
//                let unPickledData = pickler.UnPickle<int*Metric> data
                printfn "data gotten from Kinesis: %A" data
                Async.RunSynchronously <| Async.Sleep 1000
            | false -> 
                printfn "records had nothing in them"

    member self.consume () = 
        let client = self.createClient ()
        let shardId = self.getShardId (client)
        printfn "Shard ID: %A" shardId
        let shardIterator = self.getShardIterator (client, shardId)
        printfn "Shard Iterator: %A" shardIterator
        let record = self.getRecords (client, shardIterator)
        record
