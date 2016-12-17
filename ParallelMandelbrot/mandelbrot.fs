namespace Mandelbrot

type Line = int * byte array

module Mandelbrot = 

    open System
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit

    open System.Windows
    open System.Windows.Media
    open System.Windows.Media.Imaging

    open System.Collections.Concurrent

    let maxIter     = 1200
    let imageWidth  = 2048
    let imageHeight = 2048

    let centerX     = 0.001643721971153
    let centerY     = 0.822467633298876

    let zoomX       = 0.000000000010
    let zoomY       = 0.000000000010

    let mandelBrot (x : double) (y : double) (iter : int) =
        let mutable ix = x;
        let mutable iy = y;

        let mutable i = 0;

        // Zn+1 = Zn^2 + C
        while (i < iter) && ((ix * ix + iy * iy) < 4.0) do
            i <- i+1
            let tx = ix * ix - iy * iy + x;
            iy <- 2.0 * ix * iy + y;
            ix <- tx;
        
        // Zn+1 = Zn^2 + C
        if i = iter then 
            true 
        else
            false
      

    let generateMandelBrotData (queue: ConcurrentQueue<Line>) = 
        let tx = zoomX/float imageWidth
        let mx = centerX - zoomX / 2.0
        let ty = zoomY/float imageHeight;
        let my = centerY - zoomY / 2.0

        for ix = 0 to imageWidth-1 do
            let lineData = Array.zeroCreate (3*imageHeight)
            for iy = 0 to imageHeight-1 do

                let x = tx * float ix + mx
                let y = ty * float iy + my

                if (mandelBrot x y maxIter) then
                    lineData.[3 * iy + 0] <- (byte)0
                    lineData.[3 * iy + 1] <- (byte)0
                    lineData.[3 * iy + 2] <- (byte)0
                else
                    lineData.[3 * iy + 0] <- (byte)255
                    lineData.[3 * iy + 1] <- (byte)255
                    lineData.[3 * iy + 2] <- (byte)255
            let line : Line = (ix, lineData)
            queue.Enqueue(line)
        ()

