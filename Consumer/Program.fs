open KinesisConsumer

[<EntryPoint>]
let main argv = 

    let consumer = new KinesisConsumer ()
    consumer.consume ()

    0 // return an integer exit code

