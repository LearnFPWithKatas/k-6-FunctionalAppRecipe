namespace App.Controllers

open App
open App.DataAccessLayer
open App.DomainModels
open App.Dtos
open System.Net
open System.Web.Http
open System.Web.Http.Results

module Logger = 
    let log format (objs : obj []) = System.Diagnostics.Debug.WriteLine("[LOG] " + format, objs)
    
    let logSuccess format result = 
        let logSuccess obj = log format [| obj |]
        result |> Rop.successTee logSuccess
    
    let logFailure result = 
        let logError err = log "Error: {0}" [| sprintf "%A" err |]
        result |> Rop.failureTee (Seq.iter logError)

module ResponseBuilder = 
    type ResponseMessage = 
        | NotFound
        | BadRequest of string
        | InternalServerError of string
        | DomainEvent of DomainMessage
    
    let classify = 
        function 
        | CustomerIsRequired 
        | CustomerIdMustBePositive 
        | FirstNameIsRequired 
        | FirstNameMustNotBeMoreThan10Chars 
        | LastNameIsRequired 
        | LastNameMustNotBeMoreThan10Chars as msg -> 
            BadRequest(sprintf "%A" msg)
        
        | LastNameChanged _ as msg ->
            DomainEvent msg

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
    
    let domainEventsToStr msgs = 
        msgs
        |> List.map classify
        |> List.choose (function 
               | DomainEvent s -> Some s
               | _ -> None)
        |> List.map (sprintf "DomainEvent: %A; ")
        |> List.reduce (+)
    
    let toHttpResult (x : ApiController) msgs : IHttpActionResult = 
        match primaryResponse msgs with
        | NotFound -> upcast NotFoundResult(x)
        | BadRequest _ -> 
            let validationMsg = badRequestsToStr msgs
            upcast NegotiatedContentResult(HttpStatusCode.BadRequest, validationMsg, x)
        | InternalServerError msg -> upcast NegotiatedContentResult(HttpStatusCode.InternalServerError, msg, x)
        | DomainEvent _ -> 
            let eventsMsg = domainEventsToStr msgs
            upcast NegotiatedContentResult(HttpStatusCode.OK, eventsMsg, x)

type CustomersController(dao : ICustomerDao) as x = 
    inherit ApiController()
    
    let ok content = 
        if content = box() then OkResult(x) :> IHttpActionResult
        else NegotiatedContentResult(HttpStatusCode.OK, content, x) :> IHttpActionResult
    
    let toHttpResult = 
        x
        |> ResponseBuilder.toHttpResult
        |> Rop.valueOrDefault
    
    let notifyCustomerWhenLastNameChanged = 
        let detectEvent = 
            function 
            | LastNameChanged(oldLN, newLN) -> Some(oldLN, newLN)
            | _ -> None
        
        let insertIntoNotificationMessageQ (oldLN, newLN) = 
            Logger.log "LastName changed from {0} to {1}" [| oldLN; newLN |]
        Rop.successTee (fun (_, msgs) -> 
            msgs
            |> List.choose detectEvent
            |> List.iter insertIntoNotificationMessageQ)
    
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
        |> Logger.logSuccess "Get {0}"
        |> Rop.bind createCustomerId
        |> Rop.bind dao.GetById
        |> Rop.map DtoConverter.customerToDto
        |> Logger.logFailure
        |> Rop.map ok
        |> toHttpResult
    
    [<Route("customers/{id}")>]
    [<HttpPost>]
    member __.Post(id : int, [<FromBody>] dto : CustomerDto) : IHttpActionResult = 
        dto.Id <- id
        Rop.succeed dto
        |> Logger.logSuccess "Get {0}"
        |> Rop.bind DtoConverter.dtoToCustomer
        |> Rop.bind dao.Upsert
        |> Logger.logFailure
        |> notifyCustomerWhenLastNameChanged
        |> Rop.map ok
        |> toHttpResult
