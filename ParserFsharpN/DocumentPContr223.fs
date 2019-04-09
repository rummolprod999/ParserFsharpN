namespace ParserFsharp
open System.IO
open Newtonsoft.Json
open Newtonsoft.Json.Linq
open NewtonExt
open Microsoft.EntityFrameworkCore
open System.Linq
open System.Collections.Generic

type DocumentPcontr223() =
      static member val AddProd : int = 0 with get, set
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
                let builder = DocumentBuilder()
                use db = new PContr223Context()
                let res =
                   builder {
                    let! guid = __.item.StDString "guid" <| sprintf "guid not found %s" __.file.FullName
                    let! regNum = __.item.StDString "registrationNumber" <| sprintf "registrationNumber not found %s" __.file.FullName
                    let version = match __.item.SelectToken("version") with
                                  | null -> 1
                                  | x -> (int) x
                    let mutable cancel = 0
                    let exist = db.Contr223.Where(fun x -> x.RegNum = regNum && x.VersionNumber = version).Select(fun x -> x.Id) .Count()
                    if exist > 0 then return! Error ""
                    let maxNum = db.Contr223.Where(fun x -> x.RegNum = regNum).Select(fun x -> x.VersionNumber).DefaultIfEmpty(0) .Max()
                    if version > maxNum then
                            let contracts = db.Contr223.Where(fun x -> x.RegNum = regNum)
                            contracts |> Seq.iter (fun (x : PContr223) -> x.Cancel <- 1; db.Entry(x).State <- EntityState.Modified)
                            db.SaveChanges() |> ignore
                    else cancel <- 1
                    let mutable customer: Option<Customer> = None
                    return ""
                    }
                match res with
                | Success r -> ()
                | Error e when e = "" -> ()
                | Error r -> Logging.Log.logger r
                ()
