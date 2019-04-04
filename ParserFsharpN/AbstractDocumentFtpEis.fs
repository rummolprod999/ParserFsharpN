namespace ParserFsharp

[<AbstractClass>]
type AbstractDocumentFtpEis() =
    member val Add: int = 0
    
    abstract Parsing : unit -> unit