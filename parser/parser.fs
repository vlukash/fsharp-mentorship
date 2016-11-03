namespace Parser

type Result<'T> =
     | Success of 'T*int 
     | Failure of string list*int

type Parser<'T> = string -> int -> Result<'T>

type SingleCharParser = string -> Result<char>

module SingleChar = 
    open System

    let parseFirstCharHardcodedA (input: string) (pos: int): Result<char> =
        let charToMatch = 'a' // mrange: This can be a parameter obviously but even more powerful would be to have a test function parameter
        if String.IsNullOrEmpty(input) then
            Failure (["Input is empty"], 0)
        else
            let firstChar = input.[0]
            if firstChar = charToMatch then
                Success(charToMatch, pos+1)
            else
                let errorMessage = sprintf "Expected character is: '%c' but received '%c' at position %i" charToMatch firstChar pos
                Failure ([errorMessage], pos)
