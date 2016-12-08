namespace mandelbrot

module AkkaTests = 
    open Xunit 
    open Akka
    open System

    [<Fact>]
    let ``first test``() =
        run |> ignore
        Assert.Equal(1, 1)