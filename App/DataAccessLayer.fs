namespace App.DataAccessLayer

open App.DomainModels
open App.SqlDatabase
open System

type ICustomerDao = 
    abstract GetById : int -> Customer
    abstract Upsert : Customer -> unit

type CustomerDao() = 
    
    let fromDbCustomer (cust : DbCustomer) = 
        if isNull cust then null
        else Customer.Create(cust.Id, cust.FirstName, cust.LastName)
    
    let toDbCustomer (cust : Customer) = DbCustomer(Id = cust.Id, FirstName = cust.FirstName, LastName = cust.LastName)
    interface ICustomerDao with
        
        member __.GetById(custId : int) : Customer = 
            DbContext().Customers()
            |> Seq.tryFind (fun c -> c.Id = custId)
            |> Option.map fromDbCustomer
            |> Option.fold (fun _ -> id) null
        
        member x.Upsert(customer : Customer) : unit = 
            if isNull customer then raise <| ArgumentNullException("customer")
            else 
                let db = DbContext()
                let existingDbCust = (x :> ICustomerDao).GetById(customer.Id)
                let newDbCust = toDbCustomer customer
                if isNull existingDbCust then db.Insert(newDbCust)
                else db.Update(newDbCust)
