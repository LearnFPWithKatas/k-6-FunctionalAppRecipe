namespace App.Controllers

open App
open App.DataAccessLayer
open App.DomainModels
open App.Dtos
open System.Net
open System.Web.Http
open System.Web.Http.Results

module ResponseBuilder = 
    type ResponseMessage = 
        | NotFound
        | BadRequest of string
        | InternalServerError of string
    
    let classify = 
        function 
        | CustomerIsRequired 
        | CustomerIdMustBePositive 
        | FirstNameIsRequired 
        | FirstNameMustNotBeMoreThan10Chars 
        | LastNameIsRequired 
        | LastNameMustNotBeMoreThan10Chars as msg -> 
            BadRequest(sprintf "%A" msg)
        
        | CustomerNotFound -> NotFound
        
        | SqlCustomerIsInvalid 
        | DatabaseTimeout 
        | DatabaseError _ as msg -> 
            InternalServerError(sprintf "%A" msg)
    
    let primaryResponse = 
        List.map classify
        >> List.sort
        >> List.head
    
    let badRequestsToStr msgs = 
        msgs
        |> List.map classify
        |> List.choose (function 
               | BadRequest s -> Some s
               | _ -> None)
        |> List.map (sprintf "ValidationError: %s; ")
        |> List.reduce (+)
    
    let toHttpResult (x : ApiController) msgs : IHttpActionResult = 
        match primaryResponse msgs with
        | NotFound -> upcast NotFoundResult(x)
        | BadRequest _ -> 
            let validationMsg = badRequestsToStr msgs
            upcast NegotiatedContentResult(HttpStatusCode.BadRequest, validationMsg, x)
        | InternalServerError msg -> upcast NegotiatedContentResult(HttpStatusCode.InternalServerError, msg, x)

type CustomersController(dao : ICustomerDao) as x = 
    inherit ApiController()
    
    let ok content = 
        if content = box() then OkResult(x) :> IHttpActionResult
        else NegotiatedContentResult(HttpStatusCode.OK, content, x) :> IHttpActionResult
    
    let toHttpResult = 
        x
        |> ResponseBuilder.toHttpResult
        |> Rop.valueOrDefault
    
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
