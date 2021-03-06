﻿namespace ParserCombinator

module SingleCrar = 
    open Xunit 
    open Parser
    open System

    [<Fact>]
    let ``empty string should return failure and 0 as a pos value``() =
        let parser = singleChar 'a'
        let result = parser "" 0
        Assert.Equal(result, Failure(["Input is empty"], 0))

    [<Fact>]
    let ``non empty string, first char is 'a', should return success and incremented position value``() =
        let parser = singleChar 'a'
        let result = parser "abc" 0
        Assert.Equal(result, Success('a', 1))

    [<Fact>]
    let ``non empty string, first char is not 'a', should return failure``() =
        let parser = singleChar 'a'
        let result = parser "bca" 0
        Assert.Equal(result, Failure(["Expected character is: 'a' but received 'b' at position 0"], 0))

    // combinator tests 
    [<Fact>]
    let ``combines two parsers and runs on the correct input string``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "abc" 0
        Assert.Equal(result,  Success(('a','b'), 2))

    [<Fact>]
    let ``first parser in the chain should fail``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "dbc" 0
        Assert.Equal(result,  Failure(["Expected character is: 'a' but received 'd' at position 0"], 0))

    [<Fact>]
    let ``second parser in the chain should fail``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "adc" 0
        Assert.Equal(result,  Failure(["Expected character is: 'b' but received 'd' at position 1"], 1))

    [<Fact>]
    let ``combinator should fail on empty input string``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "" 0
        Assert.Equal(result,  Failure(["Input is empty"], 0))

    // orElse combinator tests 
    [<Fact>]
    let ``combines two parsers and runs only first one``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = orElse parser_a parser_b

        let result = combinedParser "abc" 0
        Assert.Equal(result,  Success('a', 1))

    [<Fact>]
    let ``combines two parsers, first fails, second succeeded``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = orElse parser_a parser_b

        let result = combinedParser "bdc" 0
        Assert.Equal(result,  Success('b', 1))

    [<Fact>]
    let ``both fail, error from second parser in the chain``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = orElse parser_a parser_b

        let result = combinedParser "cde" 0
        Assert.Equal(result,  Failure(["Expected character is: 'b' but received 'c' at position 0"], 0))

    [<Fact>]
    let ``orElse combinator should fail on empty input string``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = orElse parser_a parser_b

        let result = combinedParser "" 0
        Assert.Equal(result,  Failure(["Input is empty"], 0))

    let converterFunc c = int c - int '0'

    // map function tests 
    [<Fact>]
    let ``map function should convert char parser's result to int``() =

        // create char to int mapper
        let mapCharToInt = map converterFunc

        // create parser that parses char '2'
        let parser_2 = singleChar '2'

        // run parser and convert result to Int
        let result = mapCharToInt parser_2 "2abc" 0 
        Assert.Equal(result,  Success(2, 1))

    [<Fact>]
    let ``map function fails if char parser fails``() =
        // create char to int mapper
        let mapCharToInt = map converterFunc

        // create parser that parses char '2'
        let parser_2 = singleChar '2'

        // run parser and convert result to Int
        let result = mapCharToInt parser_2 "abc2" 0 
        Assert.Equal(result,  Failure(["Expected character is: '2' but received 'a' at position 0"], 0))

    // 'check' function tests
    [<Fact>]
    let ``check function should parse digit``() =
        let parser_0 = singleChar '0'
        // digit parser
        let digit = parser_0 |> check Char.IsDigit
        let result = digit "0abc" 0 
        Assert.Equal(result,  Success('0', 1))

    [<Fact>]
    let ``check function should parse digit and map to int``() =
        let parser_0 = singleChar '0'
        // integer parser
        let integer = parser_0 |> check Char.IsDigit |> map (fun ch -> int ch - int '0')
        // run parser and convert result to Int
        let result = integer "0abc" 0 
        Assert.Equal(result,  Success(0, 1))

    [<Fact>]
    let ``check function fail due to the invalid input type``() =
        let parser_a = singleChar 'a'
        // digit parser
        let digit = parser_a |> check Char.IsDigit
        let result = digit "abc0" 0 
        Assert.Equal(result,  Failure(["Invalid input: ''a'' at position 0"], 0))

    [<Fact>]
    let ``check inner parser fais``() =
        let parser_0 = singleChar '0'
        // digit parser
        let digit = parser_0 |> check Char.IsDigit
        let result = digit "abc0" 0 
        Assert.Equal(result,  Failure(["Expected character is: '0' but received 'a' at position 0"], 0))

    // 'many' function tests
    // 'many' function repeats a parser until it fails and returns the result as a list
    [<Fact>]
    let ``many function should parse 3 chars and return results as a list``() =
        let parser_a = singleChar 'a'
        // 'many' parser
        let manyP = parser_a |> many
        // run parser and convert result to Int
        let result = manyP "aaabc0" 0
        Assert.Equal(result,  Success(['a';'a';'a'], 3))

    [<Fact>]
    let ``many function should fail if internal parser fails on the first try``() =
        let parser_a = singleChar 'a'
        // 'many' parser
        let manyP = parser_a |> many
        // run parser and convert result to Int
        let result = manyP "baabc0" 0
        Assert.Equal(result,  Failure(["Expected character is: 'a' but received 'b' at position 0"], 0))

    // anyOf function tests
    [<Fact>]
    let ``anyOf parses any literal``() =
        let literal = anyOf ['a'..'z']
        let result = literal "baabc0" 0
        Assert.Equal(result,  Success('b', 1))

    [<Fact>]
    let ``anyOf parses any digit``() =
        let digit = anyOf ['0'..'9']
        let result = digit "012abc" 0
        Assert.Equal(result,  Success('0', 1))

    [<Fact>]
    let ``anyOf fails if input is not it the allowed range``() =
        let literal = anyOf ['a'..'z']
        let result = literal "01aabc" 0
        Assert.Equal(result,  Failure(["Expected character is: 'z' but received '0' at position 0"], 0))

    // parses input till chars in the allowed range
    [<Fact>]
    let ``parses input till chars in the allowed range``() =
        let literal = anyOf ['a'..'z']
        let digit = anyOf ['0'..'9']
        let any = orElse literal digit
        let result = many any "012abc" 0
        Assert.Equal(result,  Success(['0';'1';'2';'a';'b';'c'], 6))

    // parser that consumes either a char literal ('c') or a digit (3)
    [<Fact>]
    let ``parses that consumes either a char literal or a digit``() =
        // prepare integer parser
        let integerConverterFunc (c : char) = Int (int c - int '0') 
        let digit = anyOf ['0'..'9']
        let integer = digit |> map integerConverterFunc

        // prepare literal parser
        let literal = anyOf ['a'..'z'] |> map (fun c -> Literal c)

        // final parser
        let literalOrDigit = orElse literal integer

        let literaResult = literalOrDigit "abc0" 0
        let integerResult = literalOrDigit "0abc" 0
        let unsupportedSymbolRelult = literalOrDigit "#0abc" 0

        Assert.Equal(literaResult,  Success(Literal 'a', 1))
        Assert.Equal(integerResult,  Success(Int 0, 1))
        Assert.Equal(unsupportedSymbolRelult,  Failure (["Expected character is: '9' but received '#' at position 0"],0))

    // KVP parser
    [<Fact>]
    let ``parses KVP - value as an Int or String``() =
        let result_int = kvp "key_01=12345" 0
        let expected_int = KVP (Map.empty.Add("key_01", Int 12345))
        Assert.Equal(result_int, Success(expected_int, 12))

        let result_str = kvp "key_01=value" 0
        let expected_str = KVP (Map.empty.Add("key_01", Str "value"))
        Assert.Equal(result_str, Success(expected_str, 12))
