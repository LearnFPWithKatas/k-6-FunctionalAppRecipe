namespace App.DataAccessLayer

open App.DomainModels
open App.DomainModels.Primitives
open App.SqlDatabase
open System

type ICustomerDao = 
    abstract GetById : CustomerId.T -> Customer
    abstract Upsert : Customer -> unit

type CustomerDao() = 
    
    let fromDbCustomer (cust : DbCustomer) = 
        if isNull cust then raise <| ArgumentException(SqlCustomerIsInvalid.ToString())
        else 
            let id = createCustomerId cust.Id
            let fName = createFirstName cust.FirstName
            let lName = createLastName cust.LastName
            let name = createPersonalName fName lName
            createCustomer id name
    
    let toDbCustomer (cust : Customer) = 
        let dbCust = DbCustomer()
        dbCust.Id <- cust.Id |> Primitives.CustomerId.apply id
        dbCust.FirstName <- cust.Name.FirstName |> Primitives.String10.apply id
        dbCust.LastName <- cust.Name.LastName |> Primitives.String10.apply id
        dbCust
    
    interface ICustomerDao with
        
        member __.GetById custId = 
            let custIdInt = custId |> CustomerId.apply id
            
            let cust = 
                DbContext().Customers()
                |> Seq.tryFind (fun c -> c.Id = custIdInt)
                |> Option.map fromDbCustomer
            match cust with
            | Some cust -> cust
            | None -> raise <| ArgumentException(CustomerNotFound.ToString())
        
        member x.Upsert(customer : Customer) : unit = 
            let db = DbContext()
            let newDbCust = toDbCustomer customer
            try 
                let __ = (x :> ICustomerDao).GetById(customer.Id)
                db.Update(newDbCust)
            with _ -> db.Insert(newDbCust)
