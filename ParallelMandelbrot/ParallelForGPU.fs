namespace ParallelForGPU

module ParallelForGPU = 
    open System
    open System.Collections.Concurrent
    open System.Threading
    open MandelbrotTypes

    open Microsoft.FSharp.Quotations
    open Alea
    open Alea.FSharp

    [<ReflectedDefinition>]
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
    [<ReflectedDefinition>]
    let generateRowDataGPU ix tx ty mx my maxIter rowLength (lineData : byte[,]) i: unit = 
        for iy = 0 to rowLength-1 do
            let x = tx * float ix + mx
            let y = ty * float iy + my
            if (mandelbrot x y maxIter) then
                lineData.[i, 3 * iy + 0] <- (byte)0
                lineData.[i, 3 * iy + 1] <- (byte)0
                lineData.[i, 3 * iy + 2] <- (byte)0
            else
                lineData.[i, 3 * iy + 0] <- (byte)255
                lineData.[i, 3 * iy + 1] <- (byte)255
                lineData.[i, 3 * iy + 2] <- (byte)255
    
    [<ReflectedDefinition>]
    let inline kernelCustom (lineData : byte[,]) (ix : int[]) (tx : float[]) (ty : float[]) (mx : float[]) (my : float[])  (maxIter : int[]) (imageWidth : int[]) =
        let start = blockIdx.x * blockDim.x + threadIdx.x
        let stride = gridDim.x * blockDim.x
        let mutable i = start
        while i < imageWidth.Length do
            generateRowDataGPU ix.[i] tx.[i] ty.[i] mx.[i] my.[i] maxIter.[i] imageWidth.[i] lineData i
            i <- i + stride

    let kernelCustomMandelbrot : KernelDef<byte[,] -> int[] -> float[] -> float[] -> float[] -> float[] -> int[] -> int[]-> unit> = 
        <@ kernelCustom @> |> Compiler.makeKernel


    let run imageWidth imageHeight (taskQueue : ConcurrentQueue<ParallelTask>) (enqueue: Line -> unit) (completed : unit -> unit) = 
        let lineDataArray : byte[,] = Array2D.zeroCreate imageHeight (imageWidth * 3)
        
        let ixArray : int[] = Array.zeroCreate imageHeight
        let txArray : float[] = Array.zeroCreate imageHeight
        let tyArray : float[] = Array.zeroCreate imageHeight
        let mxArray : float[] = Array.zeroCreate imageHeight
        let myArray : float[] = Array.zeroCreate imageHeight
        let maxIterArray : int[] = Array.zeroCreate imageHeight
        let imageWidthArray : int[] = Array.zeroCreate imageHeight

        let rec dequeueTasks (queue : ConcurrentQueue<ParallelTask>) i = 
            let (success, res) = queue.TryDequeue()
            if success then
                let (ix, tx, ty, mx, my, maxIter,imageWidth) = res
                ixArray.[i] <- ix
                txArray.[i] <- tx
                tyArray.[i] <- ty
                mxArray.[i] <- mx
                myArray.[i] <- my
                maxIterArray.[i] <- maxIter
                imageWidthArray.[i] <- imageWidth
                dequeueTasks queue (i + 1)
            else ()
        dequeueTasks taskQueue 0

        let gpu = Gpu.Default

        let lp = LaunchParam(16, 256)

        let dlineDataArray = gpu.Allocate(lineDataArray)

        let dixArray = gpu.Allocate<int>(ixArray)
        let dtxArray = gpu.Allocate<float>(txArray)
        let dtyArray = gpu.Allocate<float>(tyArray)
        let dmxArray = gpu.Allocate<float>(mxArray)
        let dmyArray = gpu.Allocate<float>(myArray)
        let dmaxIterArray = gpu.Allocate<int>(maxIterArray)
        let dimageWidthArray = gpu.Allocate<int>(imageWidthArray)

        gpu.Launch kernelCustomMandelbrot lp dlineDataArray dixArray dtxArray dtyArray dmxArray dmyArray dmaxIterArray dimageWidthArray 

        let actualLineData = Gpu.CopyToHost(dlineDataArray)
        for i = 0 to imageHeight - 1 do
            let arr = actualLineData.[i..i, *] |> Seq.cast<byte> |> Seq.toArray
            let line : Line = (i,  arr)    
            enqueue line
        completed ()

