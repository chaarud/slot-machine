module KinesisProvider

open Account
open Metric
open Nessos.FsPickler
open FSharp.Data

open System.Text
open System.Net
open System.IO
open System

let datetime (dt:System.DateTime) = 
    let date = String.Format("{0:yyyyMMdd}", dt)
    let time = String.Format("{0:HHmmss}", dt)
    date + "T" + time + "Z"

let date (dt:System.DateTime) = 
    String.Format("{0:yyyyMMdd}", dt)

let host = "kinesis.us-east-1.amazonaws.com"

let bytesToHexStr (bytes:byte array) =
    bytes
    |> Array.map (fun (x:byte) -> System.String.Format("{0:X2}", x))
    |> String.concat System.String.Empty

let createCanonicalRequest (payload:string) datetime =
    let hasher = new System.Security.Cryptography.SHA256Managed ()
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
    let signer = keySign <| System.Security.Cryptography.KeyedHashAlgorithm.Create("HmacSHA256")
    let signers = List.map signer [date; "us-east-1"; "kinesis"; "aws4_request"; (createStringToSign payload datetime date)]
    let hexSig = 
        System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/secret_access_key.config"
        |> (+) "AWS4"
        |> Encoding.UTF8.GetBytes
        |> List.reduce (>>) signers
        |> bytesToHexStr
    hexSig.ToLower ()

let sigInfo payload datetime date = 
    let accessKeyId = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/access_key_id.config"
    "AWS4-HMAC-SHA256 Credential=" + accessKeyId + "/" + date + "/us-east-1/kinesis/aws4_request, SignedHeaders=host;x-amz-date, Signature=" + (calculateSignature payload datetime date)

let provide (data:byte array) = 
    let now = System.DateTime.UtcNow
    let datetime = datetime now
    let date = date now

    //to remove
    //let data = Encoding.ASCII.GetBytes "asldfjkasldfjalsa"
    let data' = System.Convert.ToBase64String data

    let url = "https://" + host
    let body = "{\"StreamName\": \"SlotMachineProducerStream\",\"Data\": \"" + data' + "\",\"PartitionKey\": \"partition0\"}"
    let headers = [
        ("Host", host)
        ("Content-Type","application/x-amz-json-1.1")
        ("User-Agent","Amazon Kinesis")
//        ("Content-Length", (Array.length <| Encoding.ASCII.GetBytes body).ToString())
//        ("Connection","Keep-Alive")
        ("X-Amz-Target","Kinesis_20131202.PutRecord")
        ("X-Amz-Date", datetime)
        ("Authorization", sigInfo body datetime date)
        ]
    let result = Http.RequestStream (url, httpMethod = "POST", body = (TextRequest body), headers = headers)
    let reader = new StreamReader(result.ResponseStream)
    let x = reader.ReadToEnd()
    printfn "%A" x
    x