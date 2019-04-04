namespace ParserFsharp
open System

module Executor =
    let arguments = "pcontr223 last, pcontr223 daily"

    let parserArgs = function
                     | [| "pcontr223"; "last" |] -> S.argTuple <- Argument.Pcotntr223("last")
                     | [| "pcontr223"; "daily" |] -> S.argTuple <- Argument.Pcotntr223("daily")
                     | _ -> printf "Bad arguments, use %s" arguments
                            Environment.Exit(1)
    let parser = function
                 | Pcotntr223 d ->
                     P.parserPContr d
                 | _ -> ()
