namespace ParserFsharp
open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq

module NewtonExt =
    type JToken with
        member this.StDString (path : string) (err : string) =
            match this.SelectToken(path) with
            | null -> Error err
            | x -> Success (((string) x).Trim())
        
        member this.StDInt (path : string) (err : string) =
            match this.SelectToken(path) with
            | null -> Error err
            | x -> Success ((int) x)