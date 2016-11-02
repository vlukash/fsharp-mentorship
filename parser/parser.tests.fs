namespace Parser

module SingleCrar = 
    open Xunit 
    open SingleChar

    [<Fact>]
    let ``non empty string, first char is 'a', should return success and remaining string``() =
        Assert.Equal(parseFirstCharHardcodedA "abc", Success('a',"bc"))

    [<Fact>]
    let ``empty string should return failure``() =
        Assert.Equal(parseFirstCharHardcodedA "", Failure("Input is empty"))

    [<Fact>]
    let ``non empty string, first char is not 'a', should return failure``() =
        Assert.Equal(parseFirstCharHardcodedA "bca", Failure("Expected character is: 'a' but received 'b'"))