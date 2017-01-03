namespace AkkaNET

module AkkaNET = 
    open System
    open System.Collections.Concurrent
    open MandelbrotTypes

    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit

    open SingleThreaded

    let threadsCount = 4

    type WorkerMsg = 
        | InitWorker
        | CalculateBlock of ParallelTask

    type SupervisorMsg = 
        | RequestTask of IActorRef
        | CalculatedResult of ParallelTaskResult

    type Worker(name) =
        inherit Actor()

        let context = Worker.Context

        override x.OnReceive message =
            match message with
            | :? WorkerMsg as msg -> 
                match msg with
                | InitWorker ->
                    let requestTaskMsg : SupervisorMsg = RequestTask context.Self
                    context.Parent <! requestTaskMsg
                | CalculateBlock blockParams ->
                    let (ix, tx, ty, mx, my, maxIter, imageWidth) = blockParams
                    // mandelbrot computation here
                    let line = SingleThreaded.generateRowData ix tx ty mx my maxIter imageWidth 
                    let resultData : SupervisorMsg = CalculatedResult line
                    context.Parent <! resultData

                    let requestTaskMsg : SupervisorMsg = RequestTask context.Self
                    context.Parent <! requestTaskMsg
            | _ ->  failwith "unknown message"
        override x.PostStop () =
            ()

    type Supervisor(taskQueue : ConcurrentQueue<ParallelTask>, enqueue: Line -> unit, completed : unit -> unit) =
        inherit Actor()

        // Active workers counter
        let mutable activeWorkersCount = 0

        // Init child actors
        let context = Supervisor.Context
        let workers = 
            [0 .. (threadsCount - 1)]
            |> List.map(fun id ->   
                let properties = [| id :> obj |] 
                context.ActorOf(Props(typedefof<Worker>, properties)))

        override x.PreStart () = 
            // initialize workers by sending a message to each worker
            let rec initWorkers (count : int) =
                let message = WorkerMsg.InitWorker
                if count >= 0 then
                    count |> List.nth workers <! message
                    activeWorkersCount <- activeWorkersCount + 1 
                    initWorkers (count - 1)
            initWorkers (threadsCount - 1) 

        override x.OnReceive message =
            match message with
            | :? SupervisorMsg as msg -> 
                match msg with
                | RequestTask actorRef->
                    let (success, res) = taskQueue.TryDequeue()
                    if success then 
                        let task : WorkerMsg = CalculateBlock res
                        actorRef <! task
                    else 
                        //shutdown current worker
                        actorRef <! PoisonPill.Instance
                        activeWorkersCount <- activeWorkersCount - 1
                        if activeWorkersCount = 0 then
                            completed ()
                | CalculatedResult result ->
                    let line : Line = result
                    enqueue line
            | _ ->  failwith "unknown message"

    let run taskQueue (enqueue: Line -> unit) (completed : unit -> unit) = 
        let system = ActorSystem.Create("Akka")
        let supervisor = system.ActorOf(Props(typedefof<Supervisor>, [| (taskQueue) :> obj; (enqueue) :> obj; (completed) :> obj|] ))
        ()