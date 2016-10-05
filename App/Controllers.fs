namespace App.Controllers

open App
open App.DataAccessLayer
open App.DomainModels
open App.Dtos
open System.Net
open System.Web.Http
open System.Web.Http.Results

type CustomersController(dao : ICustomerDao) as x = 
    inherit ApiController()
    
    let ok content = 
        if content = box() then OkResult(x) :> IHttpActionResult
        else NegotiatedContentResult(HttpStatusCode.OK, content, x) :> IHttpActionResult
    
    let toHttpResult = 
        let msgToHttpRes msg : IHttpActionResult = 
            upcast NegotiatedContentResult(HttpStatusCode.InternalServerError, msg, x)
        Rop.valueOrDefault msgToHttpRes
    
    [<Route("example")>]
    [<HttpGet>]
    member __.GetExample() : IHttpActionResult = 
        let dto = new CustomerDto()
        dto.FirstName <- "Alice"
        dto.LastName <- "InWonderLand"
        ok dto
    
    [<Route("customers/{id}")>]
    [<HttpGet>]
    member __.Get(id : int) : IHttpActionResult = 
        Rop.succeed id
        |> Rop.bind createCustomerId
        |> Rop.bind dao.GetById
        |> Rop.map DtoConverter.customerToDto
        |> Rop.map ok
        |> toHttpResult
    
    [<Route("customers/{id}")>]
    [<HttpPost>]
    member __.Post(id : int, [<FromBody>] dto : CustomerDto) : IHttpActionResult = 
        dto.Id <- id
        Rop.succeed dto
        |> Rop.bind DtoConverter.dtoToCustomer
        |> Rop.bind dao.Upsert
        |> Rop.map ok
        |> toHttpResult
