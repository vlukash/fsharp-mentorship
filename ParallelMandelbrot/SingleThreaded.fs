namespace SingleThreaded

module SingleThreaded = 
    open System
    open MandelbrotTypes
     
    let norm (ix : float) (iy : float) : float =
        (ix * ix + iy * iy)

    let rec mandelbrotIterator (x : double) (y : double) (cx : double) (cy : double) (iter : int) =
        if (norm x y) > 4.0 then false else
            match iter with
                | 0 -> true
                | iter -> 
                    let ix = x * x - y * y + cx
                    let iy = 2.0 * x * y + cy 
                    mandelbrotIterator ix iy cx cy (iter-1)

    // generate 1 row of data
    let generateRowData ix tx ty mx my maxIter (rowLength : int) : Line = 
        let lineData = Array.zeroCreate (3*rowLength)
        for iy = 0 to rowLength-1 do

            let x = tx * float ix + mx
            let y = ty * float iy + my

            if (mandelbrotIterator x y x y maxIter) then
                lineData.[3 * iy + 0] <- (byte)0
                lineData.[3 * iy + 1] <- (byte)0
                lineData.[3 * iy + 2] <- (byte)0
            else
                lineData.[3 * iy + 0] <- (byte)255
                lineData.[3 * iy + 1] <- (byte)255
                lineData.[3 * iy + 2] <- (byte)255
        let line : Line = (ix, lineData)
        line

