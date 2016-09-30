module App.DomainModels

[<AllowNullLiteral>]
type Customer private () = 
    member val Id = 0 with get, set
    member val FirstName : string = null with get, set
    member val LastName : string = null with get, set
    static member Create(id, firstName, lastName) : Customer = 
        if id = 0 || isNull firstName || isNull lastName then null
        else Customer(Id = id, FirstName = firstName, LastName = lastName)
