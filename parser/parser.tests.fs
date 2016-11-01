namespace Parser

module SingleCrar = 
    open Xunit 
    open SingleChar

    [<Fact>]
    let parse_non_empty_string_should_return_success_and_first_char() =
        Assert.Equal(parseChar "abc", Success('a'))

    [<Fact>]
    let parse_empty_string_should_return_failure() =
        Assert.Equal(parseChar "", Failure("Input is empty"))