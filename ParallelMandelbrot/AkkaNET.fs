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
            //let tid = Threading.Thread.CurrentThread.ManagedThreadId
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

    type Supervisor(taskQueue : ConcurrentQueue<ParallelTask>, queue: ConcurrentQueue<Line>) =
        inherit Actor()

        // Init child actors
        let context = Supervisor.Context
        let workers = 
            [0 .. 3]
            |> List.map(fun id ->   
                let properties = [| id :> obj |] 
                context.ActorOf(Props(typedefof<Worker>, properties)))

        override x.PreStart () = 
            // initialize workers my sending a message to each worker
            for id in [0 .. 3] do
                let message = WorkerMsg.InitWorker
                id |> List.nth workers <! message
            ()

        override x.OnReceive message =
            match message with
            | :? SupervisorMsg as msg -> 
                match msg with
                | RequestTask actorRef->
                    let (success, res) = taskQueue.TryDequeue()
                    if success then 
                        let task : WorkerMsg = CalculateBlock res
                        actorRef <! task
                | CalculatedResult result ->
                    let line : Line = result
                    queue.Enqueue(line)
            | _ ->  failwith "unknown message"

    ///////////////
    let run taskQueue queue = 
        let system = ActorSystem.Create("Akka")
        
        let supervisor = system.ActorOf(Props(typedefof<Supervisor>, [| (taskQueue) :> obj; (queue) :> obj|] ))

        //val q : ConcurrentQueue<Line>
        
        Threading.Thread.Sleep(1000)

        //system.Shutdown()

//    let addTask ix tx ty mx my maxIter imageWidth = 
//        let task : ParallelTask = (ix, tx, ty, mx, my, maxIter, imageWidth)
//        taskQueue.Enqueue(task)

