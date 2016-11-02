namespace Parser

type Result<'T> =
     | Success of 'T*string 
     | Failure of string 

type SingleCharParser = string -> Result<char>

module SingleChar = 
    open System

    let parseFirstCharHardcodedA str =
        let charToMatch = 'a'
        if String.IsNullOrEmpty(str) then
            Failure "Input is empty"
        else
            let firstChar = str.[0]
            if firstChar = charToMatch then
                Success(charToMatch,str.[1..])
            else
                let errorMessage = sprintf "Expected character is: '%c' but received '%c'" charToMatch firstChar
                Failure errorMessage