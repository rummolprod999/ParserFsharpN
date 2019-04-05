namespace ParserFsharp
open System.Collections.Generic
open System.Linq

[<AbstractClass>]
type AbstractParserFtpEis() =
    abstract member GetRegions: unit -> IEnumerable<Region>
    default this.GetRegions() =
        use context = new RegionContext()
        let regions: IEnumerable<Region> = context.Regions.ToList() :> IEnumerable<Region>
        regions