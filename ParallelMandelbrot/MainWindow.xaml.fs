namespace ViewModels

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.Windows
open System.Windows.Media
open System.Windows.Media.Imaging

open FSharp.ViewModule
open FSharp.ViewModule.Validation
open FsXaml

open Mandelbrot

type MainView = XAML<"MainWindow.xaml", true>

type MainViewModel() as self = 
    inherit ViewModelBase()

    let info = self.Factory.Backing(<@ self.Info @>, "")
    let imageSource = self.Factory.Backing(<@ self.ImageSource @>, null)
    let mutable wBitmap : WriteableBitmap = new WriteableBitmap(Mandelbrot.imageWidth, Mandelbrot.imageHeight, 96.0, 96.0, PixelFormats.Bgr24, null)
    let queue = new ConcurrentQueue<Line> ()
    let stopwatch = new Stopwatch ()

    let render() =
        self.Info <- "Rendering"
        self.ImageSource <- wBitmap

        stopwatch.Restart(); 

        Mandelbrot.generateMandelBrotData queue

        wBitmap.Lock()
        try
            let mutable line : Line = (0, Array.zeroCreate 10)
            while (queue.TryDequeue (&line)) do
                let lineNo = fst line
                let pixels = snd  line
                if (lineNo >= 0 &&  lineNo < Mandelbrot.imageHeight &&  pixels.Length/3 = Mandelbrot.imageHeight) then
                    wBitmap.WritePixels(new Int32Rect (lineNo, 0, 1, Mandelbrot.imageHeight), pixels, 3, 0)
        finally
            wBitmap.Unlock()
            stopwatch.Stop()

        self.Info <- (sprintf "Done. Time: %i ms" stopwatch.ElapsedMilliseconds)

    member self.Info with get() = info.Value 
                     and set value = info.Value  <- value

    member self.Render = self.Factory.CommandSync(render)

    member self.ImageSource with get() = imageSource.Value 
                            and set value = imageSource.Value  <- value