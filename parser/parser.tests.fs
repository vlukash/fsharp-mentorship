namespace ParserCombinator

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
        Assert.Equal(result,  Failure(["Expected character is: 'a' but received 'c' at position 0";"Expected character is: 'b' but received 'c' at position 0"], 0))

    [<Fact>]
    let ``orElse combinator should fail on empty input string``() =
        let parser_a = singleChar 'a'
        let parser_b = singleChar 'b'
        let combinedParser = orElse parser_a parser_b

        let result = combinedParser "" 0
        Assert.Equal(result,  Failure(["Input is empty";"Input is empty"], 0))

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
