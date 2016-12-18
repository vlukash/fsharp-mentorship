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

    let centerX     = 0.001643721971153
    let centerY     = 0.822467633298876

    let zoomX       = 0.000000000010
    let zoomY       = 0.000000000010

    let taskQueue = new ConcurrentQueue<ParallelTask> ()

    let generateMandelBrotData (queue: ConcurrentQueue<Line>) (computationType : ComputationType) = 
        let tx = zoomX/float imageWidth
        let mx = centerX - zoomX / 2.0
        let ty = zoomY/float imageHeight;
        let my = centerY - zoomY / 2.0

        for ix = 0 to imageWidth-1 do
            match computationType with
                | SingleThreadType ->
                    let line = SingleThreaded.generateRowData ix tx ty mx my maxIter imageWidth
                    queue.Enqueue(line)
                | AkkaNETType ->
                    // prepare queue of tasks for Akka workers
                    //AkkaNET.addTask ix tx ty mx my maxIter imageWidth 
                    let task : ParallelTask = (ix, tx, ty, mx, my, maxIter, imageWidth)
                    taskQueue.Enqueue(task)
        if computationType = AkkaNETType then
            AkkaNET.run taskQueue queue
        while queue.Count <> 2048 do
            ()
        //Threading.Thread.Sleep(30000)
        ()

