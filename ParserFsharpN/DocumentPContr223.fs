namespace ParserFsharp
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type DocumentPcontr223() =
      [<DefaultValue>] val mutable file : FileInfo
      [<DefaultValue>] val mutable item : JToken
      [<DefaultValue>] val mutable region : Region
      new(f, i, r) as this = DocumentPcontr223()
                             then
                                 this.file <- f
                                 this.item <- i
                                 this.region <- r
      inherit AbstractDocumentFtpEis()
      interface IDocument with

            override __.Worker() =
                printfn "%s" <| __.item.ToString()
                ()
