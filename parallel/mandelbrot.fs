namespace mandelbrot

module Akka = 

    open System
    open Akka.Actor
    open Akka.Configuration
    open Akka.FSharp
    open Akka.TestKit

    open System.Drawing 
    open System.Windows.Forms

    open System.Collections.Concurrent

    type Value = int
    type SupervisorRef = IActorRef
    type ActorMsg = SupervisorRef * Value

    type Line = int * int array

//    type Worker(name) =
//        inherit Actor()
//
//        override x.OnReceive message =
//            let tid = Threading.Thread.CurrentThread.ManagedThreadId
//            match message with
//            | :? ActorMsg as msg -> 
//                let (supervisor, value) = msg
//                // mandelbrot computation here
//                //
//                supervisor <! value
//            | _ ->  failwith "unknown message"
//
//    type Supervisor(name) =
//        inherit Actor()
//
//        override x.OnReceive message =
//            match message with
//            | :? int as msg -> 
//                printfn "Hello %d " msg
//            | _ ->  failwith "unknown message"

    /////////////////

    let m_queue = new ConcurrentQueue<Line> ()

    let maxIter = 2048
    let imageWidth  = 512
    let imageHeight = 512

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
      

    let renderMandelBrot (bmp : Bitmap) = 
        let tx = zoomX/float imageWidth
        let mx = centerX - zoomX / 2.0
        let ty = zoomY/float imageHeight;
        let my = centerY - zoomY / 2.0

        for ix = 0 to imageWidth-1 do
            //let lineData = Array.create imageHeight 0
            for iy = 0 to imageHeight-1 do
                let x = tx * float ix + mx
                let y = ty * float iy + my

                if (mandelBrot x y maxIter) then
                    bmp.SetPixel(ix,iy,Color.Black)
//                    lineData.[4 * iy + 0] <- 0
//                    lineData.[4 * iy + 1] <- 0
//                    lineData.[4 * iy + 2] <- 0
//                    lineData.[4 * iy + 3] <- 0
                else
                    bmp.SetPixel(ix,iy,Color.White)
//                    lineData.[4 * iy + 0] <- 255
//                    lineData.[4 * iy + 1] <- 255
//                    lineData.[4 * iy + 2] <- 255
//                    lineData.[4 * iy + 3] <- 255
            //let line : Line = (ix, lineData)
            //m_queue.Enqueue(line)
        ()

    type VisualForm = class
        inherit Form

        val mutable bmp : Bitmap
 
        member public x.run () = 
            renderMandelBrot x.bmp
            x.Refresh()
 
        new() as x = {bmp = new Bitmap(imageWidth , imageHeight)} then
            x.Width <- imageWidth
            x.Height <- imageHeight
            x.BackgroundImage <- x.bmp
            x.run()
            x.Show()  
    end

    let run = 
        let f = new VisualForm()
        do Application.Run(f)
