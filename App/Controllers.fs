namespace App.Controllers

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
        id
        |> createCustomerId
        |> dao.GetById
        |> DtoConverter.customerToDto
        |> ok
    
    [<Route("customers/{id}")>]
    [<HttpPost>]
    member __.Post(id : int, [<FromBody>] dto : CustomerDto) : IHttpActionResult = 
        dto.Id <- id
        let cust = DtoConverter.dtoToCustomer dto
        dao.Upsert(cust)
        ok()
