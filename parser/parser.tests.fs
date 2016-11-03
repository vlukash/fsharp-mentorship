namespace Parser

module SingleCrar = 
    open Xunit 
    open SingleChar

    [<Fact>]
    let ``empty string should return failure and 0 as a pos value``() =
        let parser = parseChar 'a'
        let result = parser "" 0
        Assert.Equal(result, Failure(["Input is empty"], 0))

    [<Fact>]
    let ``non empty string, first char is 'a', should return success and incremented position value``() =
        let parser = parseChar 'a'
        let result = parser "abc" 0
        Assert.Equal(result, Success('a', 1))

    [<Fact>]
    let ``non empty string, first char is not 'a', should return failure``() =
        let parser = parseChar 'a'
        let result = parser "bca" 0
        Assert.Equal(result, Failure(["Expected character is: 'a' but received 'b' at position 0"], 0))

    // combinator tests 
    [<Fact>]
    let ``combines two parsers and runs on the correct input string``() =
        let parser_a = parseChar 'a'
        let parser_b = parseChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "abc" 0
        Assert.Equal(result,  Success(('a','b'), 2))

    [<Fact>]
    let ``first parser in the chain should fail``() =
        let parser_a = parseChar 'a'
        let parser_b = parseChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "dbc" 0
        Assert.Equal(result,  Failure(["Expected character is: 'a' but received 'd' at position 0"], 0))

    [<Fact>]
    let ``second parser in the chain should fail``() =
        let parser_a = parseChar 'a'
        let parser_b = parseChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "adc" 0
        Assert.Equal(result,  Failure(["Expected character is: 'b' but received 'd' at position 1"], 1))

    [<Fact>]
    let ``combinator should fail on empty input string``() =
        let parser_a = parseChar 'a'
        let parser_b = parseChar 'b'
        let combinedParser = pair parser_a parser_b

        let result = combinedParser "" 0
        Assert.Equal(result,  Failure(["Input is empty"], 0))