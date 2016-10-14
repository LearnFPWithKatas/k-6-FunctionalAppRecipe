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
            if i < 1 then Rop.fail IntegerError.MustBePositiveInteger
            else Rop.succeed <| CustomerId i
        
        let apply f (CustomerId i) = f i
    
    type StringError = 
        | Missing
        | MustNotBeLongerThan of int
    
    module String10 = 
        type T = 
            | String10 of string
        
        let create (s : string) = 
            match s with
            | null -> Rop.fail StringError.Missing
            | _ when s.Length > 10 -> Rop.fail (MustNotBeLongerThan 10)
            | _ -> Rop.succeed <| String10 s
        
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
    | CustomerIdMustBePositive
    | FirstNameIsRequired
    | FirstNameMustNotBeMoreThan10Chars
    | LastNameIsRequired
    | LastNameMustNotBeMoreThan10Chars
    | LastNameChanged of string * string
    | CustomerNotFound
    | SqlCustomerIsInvalid
    | DatabaseTimeout
    | DatabaseError of string

let createCustomerId customerId = 
    let map = 
        function 
        | IntegerError.Missing -> CustomerIsRequired
        | MustBePositiveInteger _ -> CustomerIdMustBePositive
    CustomerId.create customerId |> Rop.mapMessages map

let createFirstName (firstName : string) : Rop.Result<String10.T, DomainMessage> = 
    let map = 
        function 
        | StringError.Missing -> FirstNameIsRequired
        | MustNotBeLongerThan _ -> FirstNameMustNotBeMoreThan10Chars
    String10.create firstName |> Rop.mapMessages map

let createLastName lastName = 
    let map = 
        function 
        | StringError.Missing -> LastNameIsRequired
        | MustNotBeLongerThan _ -> LastNameMustNotBeMoreThan10Chars
    String10.create lastName |> Rop.mapMessages map

let createPersonalName firstName lastName = 
    { FirstName = firstName
      LastName = lastName }

let createCustomer custId name = 
    { Id = custId
      Name = name }
