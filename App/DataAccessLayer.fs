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
    
    interface ICustomerDao with
        
        member __.GetById custId = 
            let custIdInt = custId |> CustomerId.apply id
            DbContext().Customers()
            |> Seq.tryFind (fun c -> c.Id = custIdInt)
            |> Option.fold (fun _ -> fromDbCustomer) (fail CustomerNotFound)
        
        member x.Upsert(customer : Customer) = 
            let db = DbContext()
            let newDbCust = toDbCustomer customer
            
            let fSuccess _ = 
                db.Update(newDbCust)
                succeed()
            
            let fFailure _ = 
                db.Insert(newDbCust)
                succeed()
            
            (x :> ICustomerDao).GetById(customer.Id) |> either fSuccess fFailure
