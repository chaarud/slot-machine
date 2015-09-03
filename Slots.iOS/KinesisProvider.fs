module KinesisProvider

open Account
open Metric
open Nessos.FsPickler
open FSharp.Data

open System.Text
open System.Net
open System.IO
open System

let bytesToHexStr (bytes:byte array) =
    bytes
    |> Array.map (fun (x:byte) -> System.String.Format("{0:X2}", x))
    |> String.concat System.String.Empty

let createCanonicalRequest (payload:string) =
//    printfn "creating the canonical request"
    let canonical :string = "POST" + "\n" + "/" + "\n" + "" + "\n" + "host:kinesis.us-east-1.amazonaws.com\nx-amz-date:20150903T120000Z" + "\n" + "host;x-amz-date" + "\n"
    let hash = new System.Security.Cryptography.SHA256Managed ()
    let hashed = hash.ComputeHash(Encoding.ASCII.GetBytes payload)
    let hex = bytesToHexStr hashed
    let lower = hex.ToLower ()
    let finalCanonical = canonical + lower
    let finalCanonicalHashed = hash.ComputeHash(Encoding.ASCII.GetBytes finalCanonical)
    let hexFinalCanonicalHashed = bytesToHexStr finalCanonicalHashed
    let finalLower = hexFinalCanonicalHashed.ToLower ()
//    printfn "final hashed canonical %A" finalLower
    finalLower

let createStringToSign payload =
    let hashedCanonical = createCanonicalRequest payload
    let toSign = "AWS4-HMAC-SHA256" + "\n" + "20150903T120000Z" + "\n" + "20150903/us-east-1/kinesis/aws4_request" + "\n" + hashedCanonical
    printfn "String to sign %A" toSign
    toSign

let keySign (hmac:Security.Cryptography.HMAC) (data:string) (key:byte array) = 
    hmac.Key <- key
    hmac.ComputeHash (Encoding.ASCII.GetBytes data)

let calculateSignature payload = 
    printfn "calculating signature"
    let signer = 
        System.Security.Cryptography.HMACSHA256.Create ()
        |> keySign
    let secret = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/secret_access_key.config"
    let hexSig = 
        signer "20150903" (Encoding.ASCII.GetBytes ("AWS4" + secret))
        |> signer "us-east-1"
        |> signer "kinesis"
        |> signer "aws4_request"
        |> signer (createStringToSign payload)
        |> bytesToHexStr
    hexSig.ToLower ()

//    hmac.Key <- Encoding.ASCII.GetBytes ("AWS4" + kSecret)
//    let kDate = hmac.ComputeHash(Encoding.ASCII.GetBytes "20150903")
//    hmac.Key <- kDate
//    let kRegion = hmac.ComputeHash(Encoding.ASCII.GetBytes "us-east-1")
//    hmac.Key <- kRegion
//    let kService = hmac.ComputeHash(Encoding.ASCII.GetBytes "kinesis")
//    hmac.Key <- kService
//    let kSigning = hmac.ComputeHash(Encoding.ASCII.GetBytes "aws4_request")
//    hmac.Key <- kSigning
//    let signature = hmac.ComputeHash(Encoding.ASCII.GetBytes (createStringToSign payload))
//    let hexSig =
//        signature
//        |> Array.map (fun (x:byte) -> System.String.Format("{0:X2}", x))
//        |> String.concat System.String.Empty
//    let lowerHexSig = hexSig.ToLower ()
////    printfn "hex sig %A" lowerHexSig
//    lowerHexSig

let sigInfo payload = 
    printfn "getting signing info"
    let accessKeyId = System.IO.File.ReadAllText "/Users/chaaru/slot-machine/Slots.iOS/access_key_id.config"
    "AWS4-HMAC-SHA256 Credential=" + accessKeyId + "/20150903/us-east-1/kinesis/aws4_request, SignedHeaders=host;x-amz-date, Signature=" + (calculateSignature payload)

let provide' () = 
    let url = "https://kinesis.us-east-1.amazonaws.com/"
    let body = 
        """
        {"StreamName": "SlotMachineProducerStream","Data": "XzxkYXRhPl8x","PartitionKey": "partition0"}
        """
    let bodyBytes = Encoding.ASCII.GetBytes body
    let req = WebRequest.Create(url) :?> HttpWebRequest
    req.ProtocolVersion <- HttpVersion.Version11
    req.Method <- "POST"
    req.Host <- "kinesis.us-east-1.amazonaws.com"
    req.ContentType <- "application/x-amz-json-1.1"
    req.ContentLength <- int64 (Array.length bodyBytes)
    //req.Connection <- "Keep-Alive X-Amz-Target: Kinesis_20131202.PutRecord"
    req.UserAgent <- ""
    //may need date. If so, add to sign canonical headers and header names?
    //req.Date <- date //req.x-amz-Date <- ""
    //req.PreAuthenticate <- true
    let auth = sigInfo body
    printfn "here's the final auth %A" auth
    //does this need to be a base 64 string?
    req.Headers.Add ("Authentication", auth)
    req.Headers.Add ("X-Amz-Date", "20150903T120000Z")
    req.Headers.Add ("Connection", "Keep-Alive X-Amz-Target: Kinesis_20131202.PutRecord")
    //printfn "here's the request %A" (req.ToString ())
    printfn "headers %A" req.Headers.AllKeys

    let reqStream = req.GetRequestStream ()
    reqStream.Write (bodyBytes, 0, Array.length bodyBytes)
    reqStream.Close ()
//    printfn "closed req stream"

    let resp = 
        try
            req.GetResponse ()
        with
            | :? System.Net.WebException as ex -> 
                printfn "web exception %A" ex.Message
                ex.Response
    let stream = resp.GetResponseStream ()
    let reader = new StreamReader (stream)
    reader.ReadToEnd ()
