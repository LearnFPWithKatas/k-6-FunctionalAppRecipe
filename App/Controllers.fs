namespace App.Controllers

open App.Dtos
open System.Net
open System.Web.Http
open System.Web.Http.Results

type CustomersController() as x = 
    inherit ApiController()
    
    let ok content = 
        if content = box() then OkResult(x) :> IHttpActionResult
        else NegotiatedContentResult(HttpStatusCode.OK, content, x) :> IHttpActionResult
    
    [<Route("example")>]
    [<HttpGet>]
    member __.GetExample() : IHttpActionResult = 
        let dto = new CustomerDto()
        dto.FirstName <- "Alice"
        dto.LastName <- "InWonderLand"
        ok dto
