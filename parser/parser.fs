namespace ParserCombinator

type Result<'T> =
     | Success of 'T*int
     | Failure of string list*int

type Parser<'T> = string -> int -> Result<'T>

type Value =
    | Int of int
    | Literal of char
    | Str of string
    | KVP of Map<string, Value>

// mrange: You don't want a specific type for single char parsers. In this case it doesn't matter because it's an alias
// mrange: A parser is function that given a string and a position optionally produces a value (maybe a char) and a position
// vlukash: removed

module Parser =
    open System

    // simple one char parser
    let singleChar charToMatch : Parser<char> =
      fun input pos ->
          //let charToMatch = 'a' // mrange: This can be a parameter obviously but even more powerful would be to have a test function parameter
          if String.IsNullOrEmpty(input) then
              Failure (["Input is empty"], pos)
          else
              if pos >= input.Length then
                  Failure (["Position is out of range"], pos)
              else 
                  let firstChar = input.[pos]
                  if firstChar = charToMatch then
                      Success(charToMatch, pos+1)
                  else
                      let errorMessage = sprintf "Expected character is: '%c' but received '%c' at position %i" charToMatch firstChar pos
                      Failure ([errorMessage], pos)


    // combinator function that runs two parsers and returns the result as a pair
    let pair (l : Parser<'L>) (r : Parser<'R>) : Parser<'L*'R> =
      fun input pos ->
        // mrange: looks about right but see my comment below
        // call l parser
        match l input pos with
            | Success (l_result, l_pos) ->
                // call r parser
                match r input l_pos with
                    | Success (r_result, r_pos) ->
                        Success ((l_result, r_result), r_pos) // returning combined result and updated position
                    | Failure (err_msg, r_pos) ->
                        Failure (err_msg, r_pos) // mrange: I would use l_pos here
                                                 // vlukash: fixed, I've missed that names are the same in this result and in the first match
                                                 // mrange: My bad, it should be r_pos
            | Failure (err_msg, l_pos) ->
                Failure (err_msg, l_pos)

    // combinator function that takes two parsers and runs the first successful result
    let orElse (l : Parser<'T>) (r : Parser<'T>) : Parser<'T> =
        fun input pos ->
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
                            Failure (r_err_msg, pos) // mrange: Actually here it would be correct to merge teh error results.
                                                                 // vlukash: concatenating two lists by using @
                                                                 // vlukash: but now for an empty input it'll return ["Input is empty";"Input is empty"]. But probably that's fine
                                                                 // mrange: Merging two errors isn't as easy as this if the positions are different. Then a decent approximization could be
                                                                 //         to use the error result for the parser that got furtherest
                                                                 // vlukash: now returning only one error from r parser to make anyOf function work

    // function that maps the result of parser into another type
    let map converterFunc (p : Parser<'T>) : Parser<'U> =
      fun input pos ->
          match p input pos with
              | Success (result, s_pos) ->
                  let convertedValue = converterFunc result
                  Success (convertedValue, s_pos)
              | Failure (err_msg, f_pos) ->
                  Failure (err_msg, f_pos)

    // function that checks the input element for validity
    let check (filterFunc : 'T -> bool) (p : Parser<'T>) : Parser<'T> = 
        fun input pos ->
            match p input pos with
                | Success (convertedValue, s_pos) ->
                    // now do a validity check
                    match filterFunc convertedValue with
                        | true ->
                            Success (convertedValue, s_pos) 
                        | false ->
                            let errMsg = sprintf "Invalid input: '%A' at position %i" convertedValue pos
                            Failure ([errMsg], pos)
                | Failure (err_msg, pos) ->
                    Failure (err_msg, pos)

    // function that repeats a parser until it fails and returns the result as a list
    let many (p : Parser<'T>) : Parser<'T list> = 
        fun input pos ->
            let resultList = []
            let rec matchElement p input pos resultList = 
                match p input pos with
                | Success (result, s_pos) ->
                    let newList = result :: resultList
                    matchElement p input s_pos newList // call recursively
                | Failure (errMsg, f_pos) ->
                    match resultList with
                    | [] -> Failure (errMsg, pos) // if 0 mathes then return failure
                    | _ -> Success (List.rev resultList, pos) // in all other cases - return success with results list
            matchElement p input pos resultList

    // function that tries to parse any char from the given list
    let anyOf (charRange : char list) = 
        charRange |> List.map singleChar |> List.reduce orElse

    // Digit parser
    let digit = anyOf ['0'..'9']

    // literal parser
    let literal = anyOf ['a'..'z'] |> map (fun c -> Literal c)

    // Char parser - any char but not '='
    let char = orElse (anyOf ['!'..'<']) (anyOf ['>'..'~'])

    let fromTuple ((c1, c2), c3) : Value =
        let map = Map.empty.Add(c1, c3)
        KVP map

    // prepare Key parser
    let key = char |> many |> map (fun clist -> clist |> List.toArray |> (fun s -> new System.String(s)))

    // '=' parser
    let equal = singleChar '='

    // prepare String parser
    let string = char |> many |> map (fun clist -> clist |> List.toArray |> (fun s -> Str (new System.String(s))))
    // Int parser
    let integer = digit |> many |> map (fun clist -> clist |> List.toArray |> (fun s -> new System.String(s))) |> map (fun c -> Int (int c))

    // prepare Value parser - String or Int
    let value = orElse integer string

    let kvpTuple = pair (pair key equal) value

    // final KVP parser
    let kvp = kvpTuple |> map fromTuple

    
    // return
    // val return_: 'T -> Parser<'T>
    let return_ value : Parser<'T> = 
        let intern input pos = 
            Success (value, pos)
        intern

    // bind
    // val bind: Parser<'T> -> ('T -> Parser<'U>) -> Parser<'U>
    let bind (parser : Parser<'T>) (u:'T -> Parser<'U>) : Parser<'U> =
        let intern input pos =
            let result = parser input pos
            match result with
                | Success(value, pos) ->
                    u value input pos
                    //Success (value, pos)
                | Failure (errMsg, f_pos) ->
                    Failure (errMsg, f_pos)
        intern

    let (>>=) f a = bind f a

    type Builder() = 
        member x.Bind(f, a) = 
            bind f a

        member x.Return(f) = 
            return_ f

    let parser = Builder()

    let p = parser {
        let! t = singleChar 't'
        //code
        let! e = singleChar 'e'
        return t, e
    }

    let p2 = (singleChar 't') >>= (fun t -> singleChar 'e' >>= (fun e -> return_ (t, e)))
    
    let res = p "test" 0
    let res2 = p2 "test" 0

