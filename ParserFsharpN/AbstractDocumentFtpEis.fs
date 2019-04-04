namespace ParserFsharp

[<AbstractClass>]
type AbstractDocumentFtpEis() =
    static member val Add: int = 0 with get, set
    static member val Upd: int = 0 with get, set
    abstract Parsing : unit -> unit