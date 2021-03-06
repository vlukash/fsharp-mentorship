﻿namespace Mandelbrot

module Mandelbrot = 
    open System
    open System.Collections.Concurrent

    open MandelbrotTypes
    open SingleThreaded
    open AkkaNET
    open ParallelForGPU

    let maxIter     = 9200
    let imageWidth  = 2048
    let imageHeight = 2048

    let centerX     = -0.5
    let centerY     = 0.05

    let zoomX       = 2.0
    let zoomY       = 2.0

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
                | ParallelForGPUType ->
                    let task : ParallelTask = (ix, tx, ty, mx, my, maxIter, imageWidth)
                    taskQueue.Enqueue(task)

        
        match computationType with
            | SingleThreadType ->
                completed ()
            | AkkaNETType ->
                AkkaNET.run taskQueue enqueue completed
            | ParallelForGPUType ->
                ParallelForGPU.run imageWidth imageHeight taskQueue enqueue completed              