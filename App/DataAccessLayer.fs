namespace App.DataAccessLayer

open App.DomainModels
open App.DomainModels.Primitives
open App.Rop
open App.SqlDatabase

type ICustomerDao = 
    abstract GetById : CustomerId.T -> Result<Customer, DomainMessage>
    abstract Upsert : Customer -> Result<unit, DomainMessage>

type CustomerDao() = 
    
    let fromDbCustomer (cust : DbCustomer) = 
        if isNull cust then fail SqlCustomerIsInvalid
        else 
            let name = 
                createPersonalName 
                <!> (createFirstName cust.FirstName) 
                <*> (createLastName cust.LastName)
            
            createCustomer 
            <!> (createCustomerId cust.Id) 
            <*> name

    let toDbCustomer (cust : Customer) = 
        let dbCust = DbCustomer()
        dbCust.Id <- cust.Id |> Primitives.CustomerId.apply id
        dbCust.FirstName <- cust.Name.FirstName |> Primitives.String10.apply id
        dbCust.LastName <- cust.Name.LastName |> Primitives.String10.apply id
        dbCust
    
    let failureFromException (ex : SqlException) = 
        match ex.Data0 with
        | "Timeout" -> fail DatabaseTimeout
        | _ -> fail (DatabaseError ex.Message)
    
    interface ICustomerDao with
        
        member __.GetById custId = 
            try 
                let custIdInt = custId |> CustomerId.apply id
                DbContext().Customers()
                |> Seq.tryFind (fun c -> c.Id = custIdInt)
                |> Option.fold (fun _ -> fromDbCustomer) (fail CustomerNotFound)
            with :? SqlException as ex -> failureFromException ex
        
        member x.Upsert(customer : Customer) = 
            try 
                let db = DbContext()
                let newDbCust = toDbCustomer customer
                
                let fSuccess (currentCust, _) = 
                    db.Update(newDbCust)
                    let currentLN = currentCust.Name.LastName |> String10.apply id
                    if newDbCust.LastName <> currentLN then
                        let event = LastNameChanged (currentLN, newDbCust.LastName)
                        succeedWithMsg () event 
                    else 
                        succeed ()
                
                let fFailure _ = 
                    db.Insert(newDbCust)
                    succeed()
                
                (x :> ICustomerDao).GetById(customer.Id) |> either fSuccess fFailure
            with :? SqlException as ex -> failureFromException ex
