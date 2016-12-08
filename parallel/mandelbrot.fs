namespace mandelbrot

module Akka = 

    open System
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit

    type Value = int
    type SupervisorRef = IActorRef
    type ActorMsg = SupervisorRef * Value

    type Worker(name) =
        inherit Actor()

        override x.OnReceive message =
            let tid = Threading.Thread.CurrentThread.ManagedThreadId
            match message with
            | :? ActorMsg as msg -> 
                let (supervisor, value) = msg
                // mandelbrot computation here
                //
                supervisor <! value
            | _ ->  failwith "unknown message"

    type Supervisor(name) =
        inherit Actor()

        override x.OnReceive message =
            match message with
            | :? int as msg -> 
                printfn "Hello %d " msg
            | _ ->  failwith "unknown message"

    let run = 
        let system = ActorSystem.Create("Akka")
        let workers = 
            [1 .. 10]
            |> List.map(fun id ->   
                let properties = [| id :> obj |] 
                system.ActorOf(Props(typedefof<Worker>, properties)))
        
        let supervisor = system.ActorOf(Props(typedefof<Supervisor>, [| "main" :> obj |] ))

        for id in [1 .. 9] do
            let message = (supervisor, id)
            id |> List.nth workers <! message
        
        Threading.Thread.Sleep(1000)

        system.Shutdown()

