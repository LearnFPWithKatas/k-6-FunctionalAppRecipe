module App.DomainModels

open System

module Primitives = 
    type IntegerError = 
        | Missing
        | MustBePositiveInteger
    
    module CustomerId = 
        type T = 
            | CustomerId of int
        
        let create (i : int) = 
            if i < 1 then raise <| ArgumentException(IntegerError.MustBePositiveInteger.ToString())
            else CustomerId i
        
        let apply f (CustomerId i) = f i
    
    type StringError = 
        | Missing
        | MustNotBeLongerThan of int
    
    module String10 = 
        type T = 
            | String10 of string
        
        let create (s : string) = 
            match s with
            | null -> raise <| ArgumentException(StringError.Missing.ToString())
            | _ when s.Length > 10 -> raise <| ArgumentException((MustNotBeLongerThan 10).ToString())
            | _ -> String10 s
        
        let apply f (String10 s) = f s

open Primitives

type PersonalName = 
    { FirstName : String10.T
      LastName : String10.T }

type Customer = 
    { Id : CustomerId.T
      Name : PersonalName }

type DomainMessage = 
    | CustomerIsRequired
    | CustomerNotFound
    | SqlCustomerIsInvalid

let createCustomerId customerId = CustomerId.create customerId
let createFirstName firstName = String10.create firstName
let createLastName lastName = String10.create lastName

let createPersonalName firstName lastName = 
    { FirstName = firstName
      LastName = lastName }

let createCustomer custId name = 
    { Id = custId
      Name = name }
