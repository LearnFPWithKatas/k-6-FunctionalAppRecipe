module App.Tests

open global.Xunit
open FsUnit.Xunit
open Microsoft.Owin.Hosting
open System.Net.Http
open System


[<Fact>]
let ``Get example DTO``() =
    async {
        use __ = WebApp.Start<Startup>("http://localhost:9001")
        use client = new HttpClient(BaseAddress = Uri("http://localhost:9001"));

        let! response = client.GetAsync("example") |> Async.AwaitTask

        let! msg = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        msg |> should equal "{\r\n  \"id\": 0,\r\n  \"firstName\": \"Alice\",\r\n  \"lastName\": \"InWonderLand\"\r\n}"
    } |> Async.StartAsTask

