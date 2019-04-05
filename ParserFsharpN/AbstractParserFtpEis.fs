namespace ParserFsharp

[<AbstractClass>]
type AbstractParserFtpEis() =
    abstract member GetRegions: unit -> unit
    default this.GetRegions() =
        ()