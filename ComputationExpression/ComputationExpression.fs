namespace ComputationExpression

type Result<'T> = 
    | Ok of 'T
    | Bad of string

type DivideBy<'T> = 'T -> Result<'T>

module Result1 = 
    
    //let log value = printfn "value is %A" value

    let divideBy top : DivideBy<int> = 
        let intern bottom = 
            if bottom = 0 then
                Bad "Can't divide by 0"
            else 
                Ok (top/bottom)
        intern

    // M<'T> -> ('T -> M<'U>) -> M<'U>
    let bind_ (div: DivideBy<'T>) (f: 'T -> DivideBy<'U>) : DivideBy<'U> =
        let intern bottom = 
            let result = div bottom
            match result with
                | Ok value -> 
                    f value bottom
                | Bad err -> Bad err
        intern
        
    // 'T -> M<'T>
    let return_ (x : 'T) : Result<'T> = 
        let intern = 
            Ok x
        intern

    type Builder() = 
        member x.Bind(a, f) = 
            bind_ f a
 
        member x.Return(f) = 
            return_ f

    let res = new Builder()

    let div = res {
        let! first =  divideBy
        return first
    }

    let p = div 12

