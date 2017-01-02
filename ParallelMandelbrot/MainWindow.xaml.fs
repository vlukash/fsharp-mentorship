namespace ViewModels

open System
open System.Collections.Concurrent
open System.Diagnostics
open System.Threading
open System.Threading.Tasks
open System.Windows
open System.Windows.Media
open System.Windows.Media.Imaging

open FSharp.ViewModule
open FSharp.ViewModule.Validation
open FsXaml

open Mandelbrot
open MandelbrotTypes

type MainView = XAML<"MainWindow.xaml", true>

type MainViewModel() as self = 
    inherit ViewModelBase()

    let stopwatch = 
        let sw = new Stopwatch()
        sw.Start()
        sw

    let info = self.Factory.Backing(<@ self.Info @>, "")
    let imageSource = self.Factory.Backing(<@ self.ImageSource @>, null)
    let mutable wBitmap : WriteableBitmap = new WriteableBitmap(Mandelbrot.imageWidth, Mandelbrot.imageHeight, 96.0, 96.0, PixelFormats.Bgr24, null)

    let context = SynchronizationContext.Current

    let runInUIThread (a : unit -> unit) =
        context.Post (SendOrPostCallback (fun _ -> a ()), null)

    let updateBitmap (line : Line) =
        wBitmap.Lock()
        try
            let lineNo = fst line
            let pixels = snd line
            if (lineNo >= 0 &&  lineNo < Mandelbrot.imageHeight &&  pixels.Length/3 = Mandelbrot.imageHeight) then
                wBitmap.WritePixels(new Int32Rect (lineNo, 0, 1, Mandelbrot.imageHeight), pixels, 3, 0)
        finally
            wBitmap.Unlock()

    //let computationType = SingleThreadType
    let computationType = AkkaNETType

    let render() =
        self.Info <- "Rendering"
        self.ImageSource <- wBitmap

        let enqueue (line : Line) =
            runInUIThread (fun () -> updateBitmap line)

        let before = stopwatch.ElapsedMilliseconds

        let completed () =
            let now = stopwatch.ElapsedMilliseconds
            let diff = now - before
            runInUIThread ( fun() -> self.Info <- (sprintf "Done. Time: %i ms" diff))

        Task.Factory.StartNew (Action (fun () -> Mandelbrot.generateMandelBrotData computationType enqueue completed), TaskCreationOptions.LongRunning) |> ignore 

    member self.Info with get() = info.Value 
                     and set value = info.Value  <- value

    member self.Render = self.Factory.CommandSync(render)

    member self.ImageSource with get() = imageSource.Value 
                            and set value = imageSource.Value  <- value
