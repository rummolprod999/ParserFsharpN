namespace ParserFsharp

type DocResult<'a> =
    | Success of 'a
    | Error of string

type DocumentBuilder() =
    
    member this.Bind(m, f) =
        match m with
        | Error e -> Error e
        | Success a -> f a
    
    member this.Return(x) = Success x
    
    member this.Zero() = Success "ok"


