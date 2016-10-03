namespace App.Dtos

[<AllowNullLiteralAttribute>]
type CustomerDto() = 
    member val Id = 0 with get, set
    member val FirstName : string = null with get, set
    member val LastName : string = null with get, set

module DtoConverter = 
    open App.DomainModels
    open System
    
    let dtoToCustomer (dto : CustomerDto) = 
        if isNull dto then raise <| ArgumentException(CustomerIsRequired.ToString())
        else 
            let id = createCustomerId dto.Id
            let fName = createFirstName dto.FirstName
            let lName = createLastName dto.LastName
            let name = createPersonalName fName lName
            createCustomer id name
    
    let customerToDto (cust : Customer) = 
        let dtoCust = CustomerDto()
        dtoCust.Id <- cust.Id |> Primitives.CustomerId.apply id
        dtoCust.FirstName <- cust.Name.FirstName |> Primitives.String10.apply id
        dtoCust.LastName <- cust.Name.LastName |> Primitives.String10.apply id
        dtoCust
