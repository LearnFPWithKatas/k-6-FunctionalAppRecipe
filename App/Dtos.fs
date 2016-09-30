namespace App.Dtos

[<AllowNullLiteralAttribute>]
type CustomerDto() = 
    member val Id = 0 with get, set
    member val FirstName : string = null with get, set
    member val LastName : string = null with get, set

module DtoConverter = 
    open App.DomainModels
    
    let dtoToCustomer (dto : CustomerDto) = 
        if isNull dto then null
        else Customer.Create(dto.Id, dto.FirstName, dto.LastName)
    
    let customerToDto (cust : Customer) = 
        if isNull cust then null
        else CustomerDto(Id = cust.Id, FirstName = cust.FirstName, LastName = cust.LastName)
