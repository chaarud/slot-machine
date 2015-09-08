module KinesisProvider

open Account
open Metric
open Nessos.FsPickler
open FSharp.Data

open System.Text
open System.Net
open System.IO
open System

// Interface ================================

type HttpStatusType = 
    | OK
    | NotOK

let httpStatus = function
    | 200 -> OK
    | _ -> NotOK

type StreamName = StreamName of string

type PartitionKey = PartitionKey of string

type Request = unit -> Async<HttpResponseWithStream>

type Publisher = 
    {
        createPutRecordRequest : StreamName -> PartitionKey ->  byte array -> Request 
        publish : Request -> HttpStatusType
        publishAsync : Request -> Async<HttpStatusType>
    }

// Kinesis implementation ================================

let host = "kinesis.us-east-1.amazonaws.com"

let date (dt:DateTime) = 
    String.Format("{0:yyyyMMdd}", dt)

let datetime (dt:DateTime) = 
    let date = date dt
    let time = String.Format("{0:HHmmss}", dt)
    date + "T" + time + "Z"

let bytesToHexStr (bytes:byte array) =
    bytes
    |> Array.map (fun b -> String.Format("{0:X2}", b))
    |> String.concat String.Empty

let createCanonicalRequest (payload:string) datetime =
    let hasher = new Security.Cryptography.SHA256Managed ()
    let hexPayloadHash = 
        hasher.ComputeHash(Encoding.ASCII.GetBytes payload)
        |> bytesToHexStr

    let canonicalRequest = "POST" + "\n" + "/" + "\n" + "" + "\n" + "host:" + host + "\nx-amz-date:" + datetime + "\n" + "\n" + "host;x-amz-date" + "\n"
    let request = canonicalRequest + hexPayloadHash.ToLower()
    let hexRequest = 
        hasher.ComputeHash(Encoding.ASCII.GetBytes request)
        |> bytesToHexStr
    hexRequest.ToLower ()

let createStringToSign payload datetime date =
    let hashedCanonical = createCanonicalRequest payload datetime
    "AWS4-HMAC-SHA256" + "\n" + datetime + "\n" + date + "/us-east-1/kinesis/aws4_request" + "\n" + hashedCanonical

let keySign (hmac:Security.Cryptography.KeyedHashAlgorithm) (data:string) (key:byte array) = 
    hmac.Key <- key
    hmac.ComputeHash (Encoding.UTF8.GetBytes data)

let calculateSignature payload datetime date = 
    let signer = keySign <| Security.Cryptography.KeyedHashAlgorithm.Create("HmacSHA256")
    let signers = List.map signer [date; "us-east-1"; "kinesis"; "aws4_request"; (createStringToSign payload datetime date)]
    let hexSig = 
        IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/secret_access_key.config"
        |> (+) "AWS4"
        |> Encoding.UTF8.GetBytes
        |> List.reduce (>>) signers
        |> bytesToHexStr
    hexSig.ToLower ()

let sigInfo payload datetime date = 
    let accessKeyId = IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/access_key_id.config"
    "AWS4-HMAC-SHA256 Credential=" + accessKeyId + "/" + date + "/us-east-1/kinesis/aws4_request, SignedHeaders=host;x-amz-date, Signature=" + (calculateSignature payload datetime date)

let kinesisCreatePutRecordRequest (StreamName stream) (PartitionKey partition) data = 
    let now = System.DateTime.UtcNow
    let datetime = datetime now
    let date = date now

    let url = "https://" + host
    let body = "{\"StreamName\": \"" + stream + "\",\"Data\": \"" + (Convert.ToBase64String data) + "\",\"PartitionKey\": \"" + partition + "\"}"
    let headers = [
        ("Host", host)
        ("Content-Type","application/x-amz-json-1.1")
        ("User-Agent","Amazon Kinesis")
        ("X-Amz-Target","Kinesis_20131202.PutRecord")
        ("X-Amz-Date", datetime)
        ("Authorization", sigInfo body datetime date)
        ]
//    Http.RequestStream (url, httpMethod = "POST", body = (TextRequest body), headers = headers)
    let request () = Http.AsyncRequestStream (url, httpMethod = "POST", body = (TextRequest body), headers = headers)
    request

let kinesisPublishAsync (request : Request) =
    async {
        let! response = request () 
        return httpStatus response.StatusCode
        }

let kinesisPublish (request : Request) = 
    Async.RunSynchronously <| kinesisPublishAsync request

let kinesisPublisher =  
    {
        createPutRecordRequest = kinesisCreatePutRecordRequest
        publish = kinesisPublish
        publishAsync = kinesisPublishAsync
    }