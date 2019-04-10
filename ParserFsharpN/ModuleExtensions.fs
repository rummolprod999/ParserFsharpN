namespace ParserFsharp
open System
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open System.Globalization
open System.Text.RegularExpressions

module NewtonExt =

    let (|RegexMatch1|_|) (pattern : string) (input : string) =
        let result = Regex.Match(input, pattern)
        if result.Success then
            match (List.tail [ for g in result.Groups -> g.Value ]) with
            | fst :: [] -> Some(fst)
            | _ -> None
        else None

    let inline GetStringFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> ""
            | r -> ((string) r).Trim()

    let inline GetIntFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> 0
            | r -> ((int) r)
    
    let inline GetDecimalFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> 0m
            | r -> ((decimal) r)
            
    let inline GetDateTimeFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> DateTime.MinValue
            | r -> DateTime.Parse((string) r)

    let inline GetDateTimeStringFromJtoken (x : ^a) (s : string) =
            match (^a : (member SelectToken : string -> JToken) (x, s)) with
            | null -> ""
            | rr when (string) rr = "null" -> ""
            | r -> match JsonConvert.SerializeObject(r) with
                   | null -> ""
                   | t -> t.Trim('"')


    type JToken with
        member this.StDString (path : string) (err : string) =
            match this.SelectToken(path) with
            | null -> Error err
            | x -> Success(((string) x).Trim())

        member this.StDInt (path : string) (err : string) =
            match this.SelectToken(path) with
            | null -> Error err
            | x -> Success((int) x)

    type System.String with

        member this.Get1FromRegexp(regex : string) : string option =
            match this with
            | RegexMatch1 regex gr1 -> Some(gr1)
            | _ -> None

        member this.GetPriceFromString(?template) : string =
            let templ = defaultArg template @"([\d, ]+)"
            match this.Get1FromRegexp templ with
            | Some x -> Regex.Replace(x.Replace(",", ".").Trim(), @"\s+", "")
            | None -> ""
        
        member this.DateFromStringRus(pat : string) =
            try
                DateTime.ParseExact(this, pat, CultureInfo.CreateSpecificCulture("ru-RU"))
            with ex -> DateTime.MinValue