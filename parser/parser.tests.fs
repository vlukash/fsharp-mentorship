namespace Parser

module SingleCrar = 
    open Xunit 
    open SingleChar

    [<Fact>]
    let ``empty string should return failure and 0 as a pos value``() =
        let result = parseFirstCharHardcodedA "" 0
        Assert.Equal(result, Failure(["Input is empty"], 0))

    [<Fact>]
    let ``non empty string, first char is 'a', should return success and incremented position value``() =
        let result = parseFirstCharHardcodedA "abc" 0
        Assert.Equal(result, Success('a', 1))

    [<Fact>]
    let ``non empty string, first char is not 'a', should return failure``() =
        let result = parseFirstCharHardcodedA "bca" 0
        Assert.Equal(result, Failure(["Expected character is: 'a' but received 'b' at position 0"], 0))