module App.Tests

open global.Xunit
open FsUnit.Xunit
open Microsoft.Owin.Hosting
open System.Net.Http
open System
open System.Text

[<Fact>]
let ``Get example DTO``() =
    async {
        use __ = WebApp.Start<Startup>("http://localhost:9001")
        use client = new HttpClient(BaseAddress = Uri("http://localhost:9001"));

        let! response = client.GetAsync("example") |> Async.AwaitTask

        let! msg = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        msg |> should equal "{\r\n  \"id\": 0,\r\n  \"firstName\": \"Alice\",\r\n  \"lastName\": \"InWonderLand\"\r\n}"
    } |> Async.StartAsTask

[<Fact>]
let ``POST and then GET - Insert``() =
    async {
        use __ = WebApp.Start<Startup>("http://localhost:9001")
        use client = new HttpClient(BaseAddress = Uri("http://localhost:9001"));

        use content = new StringContent("{\"id\": 0, \"firstName\": \"Partho\", \"lastName\": \"Das\"}", Encoding.UTF8, "application/json");
        do! client.PostAsync("customers/1", content) |> Async.AwaitTask |> Async.Ignore

        let! response = client.GetAsync("customers/1") |> Async.AwaitTask
        let! msg = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        msg |> should equal "{\r\n  \"id\": 1,\r\n  \"firstName\": \"Partho\",\r\n  \"lastName\": \"Das\"\r\n}"
    } |> Async.StartAsTask

[<Fact>]
let ``POST and then GET - Update``() =
    async {
        use __ = WebApp.Start<Startup>("http://localhost:9001")
        use client = new HttpClient(BaseAddress = Uri("http://localhost:9001"));

        use content = new StringContent("{\"id\": 0, \"firstName\": \"Partho\", \"lastName\": \"Das\"}", Encoding.UTF8, "application/json");
        do! client.PostAsync("customers/1", content) |> Async.AwaitTask |> Async.Ignore

        let! response = client.GetAsync("customers/1") |> Async.AwaitTask
        let! msg = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        msg |> should equal "{\r\n  \"id\": 1,\r\n  \"firstName\": \"Partho\",\r\n  \"lastName\": \"Das\"\r\n}"

        use content = new StringContent("{\"id\": 0, \"firstName\": \"Partho P.\", \"lastName\": \"Das\"}", Encoding.UTF8, "application/json");
        do! client.PostAsync("customers/1", content) |> Async.AwaitTask |> Async.Ignore

        let! response = client.GetAsync("customers/1") |> Async.AwaitTask
        let! msg = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        msg |> should equal "{\r\n  \"id\": 1,\r\n  \"firstName\": \"Partho P.\",\r\n  \"lastName\": \"Das\"\r\n}"
    } |> Async.StartAsTask

[<Fact>]
let ``POST with invalid parameters``() =
    async {
        use __ = WebApp.Start<Startup>("http://localhost:9001")
        use client = new HttpClient(BaseAddress = Uri("http://localhost:9001"));

        use content = new StringContent("{\"id\": 0, \"firstName\": null, \"lastName\": \"Das\"}", Encoding.UTF8, "application/json");
        let! response = client.PostAsync("customers/0", content) |> Async.AwaitTask
        let! msg = response.Content.ReadAsStringAsync() |> Async.AwaitTask

        response.ReasonPhrase |> should equal "Internal Server Error"
        msg.Contains("\"FirstNameIsRequired\"") |> should equal true
        msg.Contains("\"CustomerIdMustBePositive\"") |> should equal true
    } |> Async.StartAsTask