namespace Mandelbrot

module Mandelbrot = 
    open System
    open System.Collections.Concurrent
    open MandelbrotTypes
    open SingleThreaded

    let maxIter     = 1200
    let imageWidth  = 2048
    let imageHeight = 2048

    let centerX     = 0.001643721971153
    let centerY     = 0.822467633298876

    let zoomX       = 0.000000000010
    let zoomY       = 0.000000000010

    let generateMandelBrotData (enqueue: Line -> unit) (completed : unit -> unit) = 
        let tx = zoomX/float imageWidth
        let mx = centerX - zoomX / 2.0
        let ty = zoomY/float imageHeight;
        let my = centerY - zoomY / 2.0

        for ix = 0 to imageWidth-1 do
            let line = SingleThreaded.generateRowData ix tx ty mx my maxIter imageWidth
            enqueue line

        completed ()

