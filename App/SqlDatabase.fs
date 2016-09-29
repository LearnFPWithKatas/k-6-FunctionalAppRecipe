namespace App.SqlDatabase

open System.Collections.Generic

[<AllowNullLiteralAttribute>]
type DbCustomer() = 
    member val Id = 0 with get, set
    member val Name : string = null with get, set

exception SqlException of string

type DbContext() = 
    static let _data = new Dictionary<int, DbCustomer>()
    member __.Customers() : DbCustomer seq = upcast _data.Values
    
    member __.Update(customer : DbCustomer) = 
        if _data.ContainsKey(customer.Id) |> not then raise (SqlException "KeyNotFound")
        else 
            match customer.Id with
            | 42 -> raise (SqlException "Timeout")
            | _ -> _data.[customer.Id] <- customer
    
    member __.Insert(customer : DbCustomer) = 
        if _data.ContainsKey(customer.Id) then raise (SqlException "DuplicateKey")
        else 
            match customer.Id with
            | 42 -> raise (SqlException "Timeout")
            | _ -> _data.[customer.Id] <- customer
