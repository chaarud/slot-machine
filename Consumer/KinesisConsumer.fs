module KinesisConsumer

open FSharp.Configuration

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

    member self.getShardId (client:AmazonKinesisClient) = 
        let describeStreamRequest = new Amazon.Kinesis.Model.DescribeStreamRequest ()
        describeStreamRequest.StreamName <- "SlotMachineKinesisStream"

//        might need a while loop because describestreams may not get all the shards
//        http://docs.aws.amazon.com/kinesis/latest/dev/kinesis-using-sdk-java-retrieve-shards.html
//        Alternatively, use KCL
        let describeStreamResult : DescribeStreamResponse = client.DescribeStream (describeStreamRequest)
        printfn "shards %A" describeStreamResult.StreamDescription.Shards
        let shards = describeStreamResult.StreamDescription.Shards.ToArray ()
        printfn "shards array %A" shards
        match shards.Length > 0 with
        | true -> 
            shards.[0].ShardId
        | _ -> 
            printfn "zero length array, should not be reached"
            let s = new Shard ()
            s.ShardId

    member self.getShardIterator (client:AmazonKinesisClient, shardId) = 
        let shardIteratorRequest = new Amazon.Kinesis.Model.GetShardIteratorRequest ()
        shardIteratorRequest.StreamName <- "SlotMachineKinesisStream"
        shardIteratorRequest.ShardId <- shardId
        shardIteratorRequest.ShardIteratorType <- new ShardIteratorType ("TRIM_HORIZON")

        printfn "shard iterator request: %A" shardIteratorRequest

        let shardIteratorResponse = client.GetShardIterator (shardIteratorRequest)
        let mutable shardIterator = shardIteratorResponse.ShardIterator
        shardIterator

    member self.getRecords (client:AmazonKinesisClient, shardIterator) = 
        let mutable shardIterator = shardIterator
        while true do

            let getRecordsRequest = new Amazon.Kinesis.Model.GetRecordsRequest ()
            getRecordsRequest.ShardIterator <- shardIterator
            getRecordsRequest.Limit <- 25

            let getRecordsResult : Amazon.Kinesis.Model.GetRecordsResponse = client.GetRecords (getRecordsRequest)
            let records = getRecordsResult.Records.ToArray ()
            let firstRecord = records.[0]

            shardIterator <- getRecordsResult.NextShardIterator

            let dataStream :System.IO.MemoryStream = firstRecord.Data
            let data :byte array = dataStream.ToArray ()

            printfn "data gotten from Kinesis: %A" data

            Async.RunSynchronously <| Async.Sleep 1000

    member self.consume () = 
        let client = self.createClient ()
        let shardId = self.getShardId (client)
        let shardIterator = self.getShardIterator (client, shardId)
        let record = self.getRecords (client, shardIterator)
        record
