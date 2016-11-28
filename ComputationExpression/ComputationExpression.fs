namespace ComputationExpression

type Result<'T> = 
    | Ok of 'T
    | Bad of string

type DivideBy<'T> = 'T -> Result<'T>

module Result1 = 
    
    let log value = printfn "value is %A" value

    let divideBy bottom : DivideBy<int> = 
        let intern top = 
            if bottom = 0 then
                Bad "Can't divide by 0"
            else 
                Ok (top/bottom)
        intern

    // M<'T> -> ('T -> M<'U>) -> M<'U>
    let bind_ (div: DivideBy<'T>) (f: 'T -> DivideBy<'U>) : DivideBy<'U> =
        let intern top = 
            let res = div top
            match res with
                | Ok value -> 
                    log value
                    f top value
                | Bad err -> 
                    Bad err
        intern
        
    // 'T -> M<'T>
    let return_ (x : 'T) : DivideBy<'T> = 
        let intern top = 
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
        let! first =  divideBy 5
        let! second =  divideBy 2
        let! third =  divideBy 3
        return third
    }

    let d = div 60

    let div2 = (divideBy 5) >>= (fun t -> divideBy 2 >>= (fun e -> return_ (e)))

    let d2 = div2 60

