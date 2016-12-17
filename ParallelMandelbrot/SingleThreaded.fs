namespace SingleThreaded

module SingleThreaded = 
    open System
    open MandelbrotTypes

    let mandelbrot (x : double) (y : double) (iter : int) =
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

    // generate 1 row of data
    let generateRowData ix tx ty mx my maxIter (rowLength : int) : Line = 
        let lineData = Array.zeroCreate (3*rowLength)
        for iy = 0 to rowLength-1 do

            let x = tx * float ix + mx
            let y = ty * float iy + my

            if (mandelbrot x y maxIter) then
                lineData.[3 * iy + 0] <- (byte)0
                lineData.[3 * iy + 1] <- (byte)0
                lineData.[3 * iy + 2] <- (byte)0
            else
                lineData.[3 * iy + 0] <- (byte)255
                lineData.[3 * iy + 1] <- (byte)255
                lineData.[3 * iy + 2] <- (byte)255
        let line : Line = (ix, lineData)
        line

