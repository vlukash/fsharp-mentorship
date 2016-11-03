namespace Parser

type Result<'T> =
     | Success of 'T*int 
     | Failure of string list*int

type Parser<'T> = string -> int -> Result<'T>

type SingleCharParser = string -> Result<char>

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
        // call l parser
        match l input pos with
            | Success (l_result, l_pos) -> 
                // call r parser
                match r input l_pos with
                    | Success (r_result, r_pos) -> 
                        Success ((l_result, r_result), r_pos) // returning combined result and updated position
                    | Failure (err_msg, pos) -> 
                        Failure (err_msg, pos)
            | Failure (err_msg, pos) -> 
                Failure (err_msg, pos)