namespace Parser

type Result<'T> =
     | Success of 'T*int 
     | Failure of string list*int

type Parser<'T> = string -> int -> Result<'T>

// mrange: You don't want a specific type for single char parsers. In this case it doesn't matter because it's an alias
// mrange: A parser is function that given a string and a position optionally produces a value (maybe a char) and a position
// vlukash: removed

module SingleChar = 
    open System

    // simple one char parser
    let parseChar charToMatch = 
        let parseCharInner input pos =
            //let charToMatch = 'a' // mrange: This can be a parameter obviously but even more powerful would be to have a test function parameter
            if String.IsNullOrEmpty(input) then
                Failure (["Input is empty"], pos)
            else
                let firstChar = input.[pos]
                if firstChar = charToMatch then
                    Success(charToMatch, pos+1)
                else
                    let errorMessage = sprintf "Expected character is: '%c' but received '%c' at position %i" charToMatch firstChar pos
                    Failure ([errorMessage], pos)
        parseCharInner

    // combinator function that runs two parsers and returns the result as a pair
    let pair l r input pos =
        // mrange: looks about right but see my comment below
        // call l parser
        match l input pos with
            | Success (l_result, l_pos) -> 
                // call r parser
                match r input l_pos with
                    | Success (r_result, r_pos) -> 
                        Success ((l_result, r_result), r_pos) // returning combined result and updated position
                    | Failure (err_msg, r_pos) -> 
                        Failure (err_msg, l_pos) // mrange: I would use l_pos here      
                                                 // vlukash: fixed, I've missed that names are the same in this result and in the first match                                    
            | Failure (err_msg, l_pos) -> 
                Failure (err_msg, l_pos)

    // combinator function that takes two parsers and runs the first successful result
    let orElse l r input pos = 
        // call l parser
        // mrange: Note my comments on the error results is difficult to resolve with our current parser type
        // mrange: so I say ignore them but perhaps remember it for later.
        match l input pos with
            | Success (l_result, l_pos) -> 
                Success (l_result, l_pos)                 
            | Failure (l_err_msg, l_pos) -> 
                // call r parser icase if r parser failed
                match r input pos with // mrange: You want to use pos here over l_pos.
                    | Success (r_result, r_pos) -> 
                        Success (r_result, r_pos) // mrange: Here we are now discarding an error result which can be problematic
                    | Failure (r_err_msg, pos) -> 
                        Failure (l_err_msg @ r_err_msg, pos) // mrange: Actually here it would be correct to merge teh error results.
                                                             // vlukash: concatenating two lists by using @
                                                             // vlukash: but now for an empty input it'll return ["Input is empty";"Input is empty"]. But probably that's fine
