namespace Mandelbrot

module Mandelbrot = 
    open System
    open System.Collections.Concurrent

    open MandelbrotTypes
    open SingleThreaded
    open AkkaNET

    let maxIter     = 1200
    let imageWidth  = 2048
    let imageHeight = 2048

    let centerX     = 0.001643721972268
    let centerY     = 0.822467633298876

    let zoomX       = 0.000000000008
    let zoomY       = 0.000000000008

    let taskQueue = new ConcurrentQueue<ParallelTask> ()
    let generateMandelBrotData (computationType : ComputationType) (enqueue: Line -> unit) (completed : unit -> unit) = 
        let tx = zoomX/float imageWidth
        let mx = centerX - zoomX / 2.0
        let ty = zoomY/float imageHeight;
        let my = centerY - zoomY / 2.0

        for ix = 0 to imageWidth-1 do
            match computationType with
                | SingleThreadType ->
                    let line = SingleThreaded.generateRowData ix tx ty mx my maxIter imageWidth
                    enqueue line
                | AkkaNETType ->
                    // prepare queue of tasks for Akka workers
                    let task : ParallelTask = (ix, tx, ty, mx, my, maxIter, imageWidth)
                    taskQueue.Enqueue(task)

        if computationType = AkkaNETType then
            AkkaNET.run taskQueue enqueue completed
        else completed ()