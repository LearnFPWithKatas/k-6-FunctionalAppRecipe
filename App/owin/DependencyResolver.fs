namespace App

open System
open System.Collections.Generic
open System.Web.Http.Dependencies

type TypeConstructor = unit -> obj

type DependencyResolver() = 
    let _registeredTypes = new Dictionary<Type, TypeConstructor>()
    
    let getService (serviceType : Type) = 
        let found, fn = _registeredTypes.TryGetValue(serviceType)
        if not found then null
        else fn()
    
    member __.RegisterType<'a>(ctor : TypeConstructor) = _registeredTypes.[typeof<'a>] <- ctor
    interface IDependencyResolver with
        member __.GetService(serviceType : Type) = getService serviceType
        
        member __.GetServices(serviceType : Type) = 
            let obj = getService serviceType
            if not <| isNull obj then seq { yield obj }
            else Seq.empty
        
        member __.Dispose() = ()
        member this.BeginScope() = upcast this
