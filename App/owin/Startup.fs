namespace App

open System
open System.Reflection
open System.Web.Http
open global.Owin

type ControllerResolver() = 
    inherit System.Web.Http.Dispatcher.DefaultHttpControllerTypeResolver()
    override __.GetControllerTypes(_ : System.Web.Http.Dispatcher.IAssembliesResolver) = 
        let t = typeof<System.Web.Http.Controllers.IHttpController>
        Assembly.GetExecutingAssembly().GetTypes() 
        |> Array.filter t.IsAssignableFrom 
        :> System.Collections.Generic.ICollection<Type>

type Startup() = 
    let config = new HttpConfiguration()
    let configureRoutes() = config.MapHttpAttributeRoutes()
    
    let configureDependencies() = 
        let dependencyResolver = new DependencyResolver()
        let ctor() = new Controllers.CustomersController() :> obj
        dependencyResolver.RegisterType<Controllers.CustomersController> ctor
        config.DependencyResolver <- dependencyResolver
    
    let configureJsonSerialization() = 
        let jsonSettings = config.Formatters.JsonFormatter.SerializerSettings
        jsonSettings.Formatting <- Newtonsoft.Json.Formatting.Indented
        jsonSettings.ContractResolver <- Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
    
    let configureServices() = 
        config.Services.Replace
            (typeof<System.Web.Http.Dispatcher.IHttpControllerTypeResolver>, new ControllerResolver())
    member __.Configuration(appBuilder : IAppBuilder) = 
        configureRoutes()
        configureDependencies()
        configureJsonSerialization()
        configureServices()
        config.MessageHandlers.Add(new MessageLoggingHandler())
        appBuilder.UseWebApi(config) |> ignore
