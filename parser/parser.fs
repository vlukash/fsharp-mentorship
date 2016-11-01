namespace Parser

type Result<'a> =
     | Success of 'a
     | Failure of string 

type SingleCharParser = string -> Result<char>

module SingleChar = 
    open System

    let parseChar str =
        if String.IsNullOrEmpty(str) then
            Failure "Input is empty"
        else
            Success(str.[0])