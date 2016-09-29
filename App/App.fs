module App.Entry

open System

[<EntryPoint>]
let main _ = 
    let baseAddress = "http://localhost:9001/"
    use __ = Microsoft.Owin.Hosting.WebApp.Start<Startup>(baseAddress)
    Console.WriteLine("Listening at {0}", baseAddress)
    Console.WriteLine("Press any key to stop")
    Console.ReadLine() |> ignore
    0
