﻿module KinesisProvider

open Account
open Metric
open Nessos.FsPickler
open FSharp.Data

open System.Text
open System.Net
open System.IO
open System

let datetime = "20150904T205414Z"
let date = "20150904"
let host = "kinesis.us-east-1.amazonaws.com"

let bytesToHexStr (bytes:byte array) =
    bytes
    |> Array.map (fun (x:byte) -> System.String.Format("{0:X2}", x))
    |> String.concat System.String.Empty

let createCanonicalRequest (payload:string) =
    let hasher = new System.Security.Cryptography.SHA256Managed ()
    let hashedPayload = hasher.ComputeHash(Encoding.ASCII.GetBytes payload)
    let hexPayload = bytesToHexStr hashedPayload
    let lowerCasePayload = hexPayload.ToLower ()

    let canonicalRequest :string = "POST" + "\n" + "/" + "\n" + "" + "\n" + "host:" + host + "\nx-amz-date:" + datetime + "\n" + "\n" + "host;x-amz-date" + "\n"
    let request = canonicalRequest + lowerCasePayload
    let hashedRequest = hasher.ComputeHash(Encoding.ASCII.GetBytes request)
    let hexRequest = bytesToHexStr hashedRequest
    hexRequest.ToLower ()
//    printfn "final hashed canonical %A" (hexRequest.ToLower ())

let createStringToSign payload =
    let hashedCanonical = createCanonicalRequest payload
    "AWS4-HMAC-SHA256" + "\n" + datetime + "\n" + date + "/us-east-1/kinesis/aws4_request" + "\n" + hashedCanonical

let keySign (hmac:Security.Cryptography.HMAC) (data:string) (key:byte array) = 
    let algo = "HmacSHA256"
    let kha = Security.Cryptography.KeyedHashAlgorithm.Create(algo)
    kha.Key <- key
    kha.ComputeHash (Encoding.UTF8.GetBytes data)
//    hmac.Key <- key
//    hmac.ComputeHash (Encoding.UTF8.GetBytes data)

let calculateSignature payload = 
    let signer = 
        System.Security.Cryptography.HMACSHA256.Create ()
        |> keySign
    let hexSignature = 
        System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/secret_access_key.config"
        |> (+) "AWS4"
        |> Encoding.UTF8.GetBytes
        |> signer date
        |> signer "us-east-1"
        |> signer "kinesis"
        |> signer "aws4_request"
        |> signer (createStringToSign payload)
        |> bytesToHexStr
    hexSignature.ToLower ()

let sigInfo payload = 
    let accessKeyId = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/access_key_id.config"
    "AWS4-HMAC-SHA256 Credential=" + accessKeyId + "/" + date + "/us-east-1/kinesis/aws4_request, SignedHeaders=host;x-amz-date, Signature=" + (calculateSignature payload)

let provide' () = 
    //let url = "https://kinesis.us-east-1.amazonaws.com/arn:aws:kinesis:us-east-1:567027596203:SlotMachineProducerStream"
    let url = "https://" + host
    let body = """{"StreamName": "SlotMachineProducerStream","Data": "XzxkYXRhPl8x","PartitionKey": "partition0"}"""
    let bodyBytes = Encoding.ASCII.GetBytes body

    let headers = [
        ("Host", host)
        ("Content-Type","application/x-amz-json-1.1")
        ("User-Agent","Amazon Kinesis")
//        ("Content-Length", (int64 (Array.length bodyBytes)).ToString())
//        ("Connection","Keep-Alive")
        ("X-Amz-Target","Kinesis_20131202.PutRecord")
        ("X-Amz-Date", datetime)
        ("Authorization", sigInfo body)
        ]
    let response = Http.RequestStream (url, httpMethod = "POST", body = (TextRequest body), headers = headers)
    let x = response.Headers
    let stream = response.ResponseStream
    printfn "%A" x
    printfn "%A" stream

//    let req = WebRequest.Create(url) :?> HttpWebRequest
//    req.ProtocolVersion <- HttpVersion.Version11
//    req.Method <- "POST"
//    req.Host <- "kinesis.us-east-1.amazonaws.com"
//    req.ContentType <- "application/x-amz-json-1.1"
//    req.ContentLength <- int64 (Array.length bodyBytes)
//    req.UserAgent <- "Amazon Kinesis"
//
//    //authentication header
//    let auth = sigInfo body
//    printfn "here's the final auth %A" auth
//    req.Headers.Add ("Authentication", auth)
//
//    //x-amz-Date header
//    req.Headers.Add ("X-Amz-Date", "20150904T090000Z")
//
//    //connection header?
//    req.KeepAlive <- true
////    req.Connection <- "Keep-Alive" // X-Amz-Target: Kinesis_20131202.PutRecord"
////    req.Headers.Add ("Connection", "Keep-Alive X-Amz-Target: Kinesis_20131202.PutRecord")
//    req.Headers.Add ("X-Amz-Target", "Kinesis_20131202.PutRecord")
//
//    printfn "headers %A" req.Headers.AllKeys
//    printfn "Authentication info %A" (req.Headers.Get "Authentication")
//    printfn "host %A" req.Host
//
//    let reqStream = req.GetRequestStream ()
//    reqStream.Write (bodyBytes, 0, Array.length bodyBytes)
//    reqStream.Close ()
//
//    let resp = 
//        try
//            req.GetResponse ()
//        with
//            | :? System.Net.WebException as ex -> 
//                printfn "web exception %A" ex.Message
//                ex.Response
//    let stream = resp.GetResponseStream ()

    let reader = new StreamReader (stream)
    reader.ReadToEnd ()
