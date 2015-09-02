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
        let awsAccessKeyId = "FILL_IN"//Settings.AwsAccessKeyId
        let awsSecretAccessKey = "FILL_IN"//Settings.AwsSecretAccessKey
        let region = RegionEndpoint.USEast1
        new AmazonKinesisClient (awsAccessKeyId, awsSecretAccessKey, region)

    member self.getShardId (client : AmazonKinesisClient) = 
        let describeStreamRequest = new DescribeStreamRequest ()
        describeStreamRequest.StreamName <- "SlotMachineConsumerTest3"
//        might need a while loop because describestreams may not get all the shards
//        http://docs.aws.amazon.com/kinesis/latest/dev/kinesis-using-sdk-java-retrieve-shards.html
//        Alternatively, use KCL
        let describeStreamResult : DescribeStreamResponse = client.DescribeStream (describeStreamRequest)
        let shards = describeStreamResult.StreamDescription.Shards.ToArray ()
        match shards.Length > 0 with
        | true -> 
            shards.[0].ShardId
        | _ -> 
            printfn "zero length array, should not be reached"
            let s = new Shard ()
            s.ShardId

    member self.getShardIterator (client : AmazonKinesisClient, shardId) = 
        let shardIteratorRequest = new GetShardIteratorRequest ()
        shardIteratorRequest.StreamName <- "SlotMachineConsumerTest3"
        shardIteratorRequest.ShardId <- shardId
        shardIteratorRequest.ShardIteratorType <- new ShardIteratorType ("TRIM_HORIZON") //TRIM_HORIZON vs LATEST
        let shardIteratorResponse : GetShardIteratorResponse = client.GetShardIterator (shardIteratorRequest)
        shardIteratorResponse.ShardIterator

    member self.getRecords (client : AmazonKinesisClient, shardIterator) = 
        let getRecordsRequest = new GetRecordsRequest ()
        getRecordsRequest.ShardIterator <- shardIterator
        getRecordsRequest.Limit <- 25
        let getRecordsResponse : GetRecordsResponse = client.GetRecords (getRecordsRequest)
        let records = getRecordsResponse.Records.ToArray ()
        let nextShardIterator = getRecordsResponse.NextShardIterator
        //printfn "Shard Iterator: %A" shardIterator
        match records.Length > 0 with
        | true -> 
            let firstRecord = records.[0]
            let dataStream : System.IO.MemoryStream = firstRecord.Data
            let data = dataStream.ToArray ()
            //deal with bad data in the stream
            let pickler = FsPickler.CreateBinarySerializer ()
            let unPickledData = pickler.UnPickle<int*Metric> data
            printfn "data gotten from Kinesis: %A" unPickledData
        | false -> 
            printfn "records had nothing in them"
        Async.RunSynchronously <| Async.Sleep 200
        self.getRecords (client, nextShardIterator)

    member self.consume () = 
        let client = self.createClient ()
        let shardId = self.getShardId (client)
        printfn "Shard ID: %A" shardId
        let shardIterator = self.getShardIterator (client, shardId)
        printfn "First Shard Iterator: %A" shardIterator
        self.getRecords (client, shardIterator)
