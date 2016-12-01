namespace ComputationExpression

type Result<'T> = 
    | Ok of 'T
    | Bad of string

type DivideBy<'T> = 'T -> Result<'T>

module Result = 
    
    let log value = printfn "value is %A" value

    let divideBy bottom : DivideBy<int> = 
        let intern top = 
            if bottom = 0 then
                Bad "Can't divide by 0"
            else 
                Ok (top/bottom)
        intern

    // M<'T> -> ('T -> M<'U>) -> M<'U>
    let bind_ (div: Result<'T>) (f: 'T -> Result<'U>) : Result<'U> =
        let intern = 
            match div with
                | Ok value -> 
                    log value
                    f value
                | Bad err -> 
                    Bad err
        intern
        
    // 'T -> M<'T>
    let return_ (x : 'T) : Result<'T> = 
        let intern = 
            Ok x
        intern

    type Builder() = 
        member x.Bind(f, a) = 
            bind_ f a
 
        member x.Return(f) = 
            return_ f

    let (>>=) f a = bind_ f a

    let res = new Builder()

    let div = res {
        let! first =  60 |> divideBy 5
        let! second =  first |> divideBy 2
        let! third =  second |> divideBy 3
        return third
    }

    let div2 = (60 |> divideBy 5 ) >>= (fun t -> t |> divideBy 2 >>= (fun e -> return_ (e)))


