namespace ParserFsharp
open System
open System.Linq
open System.Collections.Generic
open System.IO
open Microsoft.EntityFrameworkCore
open System.Text
open System.Xml
open Newtonsoft.Json
open Newtonsoft.Json.Linq

type ParserPcontr223(dir : string) =
      inherit AbstractParserFtpEis()
      interface Iparser with

            override __.Parsing() =
                  let regions = __.GetRegions()

                  for r in regions do
                       let mutable arr = List<string * uint64>()
                       let mutable pathParse = ""
                       match dir with

                       | "last" -> pathParse <- sprintf "/out/published/%s/purchaseContract" r.Path223
                                   arr <- __.GetArrLastFromFtp(pathParse, r.Path223)
                       | "daily" -> pathParse <- sprintf "/out/published/%s/purchaseContract/daily" r.Path223
                                    arr <- __.GetArrCurrFromFtp(pathParse, r.Path223)
                       | _ -> ()
                       if arr.Count = 0 then Logging.Log.logger (sprintf "empty array size %s" <| r.Path223)
                       for a in arr do
                             try
                                   __.GetListFA(fst a, pathParse, r)
                             with ex -> Logging.Log.logger ex
                       match dir with
                       | "daily" -> __.WriteArrToTale(arr)
                       | "last" -> __.WriteArrToTaleLast(arr)
                       | _ -> ()
                  ()

      member private __.GetListFA(arch : string, pathParse : string, reg : Region) =
            let file = __.GetArch(arch, pathParse, S.F223)
            if file.Exists then
                  let dir = __.Unzipper(file)
                  if dir.Exists then
                        let fileList = dir.GetFiles().ToList()
                        for f in fileList do
                              try
                                    __.Revision(f, pathParse, reg)
                              with ex -> Logging.Log.logger ex
                        dir.Delete(true)
                  ()
            ()

      member private __.Revision(f : FileInfo, _ : string, reg : Region) =
            match f with
            | _ when (not (f.Name.ToLower().EndsWith(".xml"))) || f.Length = 0L -> ()
            | _ -> use sr = new StreamReader(f.FullName, Encoding.Default)
                   let str = __.DeleteBadSymbols(sr.ReadToEnd())
                   __.ParsingXml(str, reg, f)
            ()
      member private __.ParsingXml(s : string, reg : Region, f : FileInfo) =
            let doc = XmlDocument()
            doc.LoadXml(s)
            let json = JsonConvert.SerializeXmlNode(doc)
            let j = JObject.Parse(json)
            match j.SelectToken("$..purchaseContract.body.item") with
            | null -> Logging.Log.logger (sprintf "Item tag not found in %s" f.FullName)
            | x -> __.Worker(DocumentPcontr223(f, x, reg))
            ()

      member private __.Worker(d : IDocument) =
            try
                  d.Worker()
            with ex -> Logging.Log.logger ex

      member private __.GetArrLastFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F223)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let _ = seq { for s in yearsSeq do yield sprintf "_%s_%d" region s }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            use context = new ArchivePContr223Context()
            let arr = List<string * uint64>()
            for r in ret do
                  match (snd r) with
                  | 0UL -> Logging.Log.logger (sprintf "!!!archive size = 0 %s" <| fst r)
                  | _ -> let arr_last = sprintf "last_%s" (fst r)
                         let res = context.Archives.AsNoTracking().Where(fun x -> x.Archive = arr_last && (uint64 x.SizeArch = (snd r) || uint64 x.SizeArch = 0UL)).Count()
                         if res = 0 then arr.Add(r)
                         ()
            arr

      member private __.GetArrCurrFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F223)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let _ = seq { for s in yearsSeq do yield sprintf "_%s_%d" region s }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            use context = new ArchivePContr223Context()
            let arr = List<string * uint64>()
            for r in ret do
                  match (snd r) with
                  | 0UL -> Logging.Log.logger (sprintf "!!!archive size = 0 %s" <| fst r)
                  | _ -> let res = context.Archives.AsNoTracking() .Where(fun x -> x.Archive = (fst r) && (uint64 x.SizeArch = (snd r) || uint64 x.SizeArch = 0UL)) .Count()
                         if res = 0 then arr.Add(r)
                         ()
            arr

      member private __.WriteArrToTale(lst : List<string * uint64>) =
          use context = new ArchivePContr223Context()
          for l in lst do
                let arr = ArchivePContr223()
                arr.Archive <- fst l
                arr.SizeArch <- int64 <| snd l
                context.Archives.Add(arr) |> ignore
                context.SaveChanges() |> ignore
                ()
          ()

      member private __.WriteArrToTaleLast(lst : List<string * uint64>) =
          use context = new ArchivePContr223Context()
          for l in lst do
                let arr = ArchivePContr223()
                let arr_last = sprintf "last_%s" (fst l)
                arr.Archive <- arr_last
                arr.SizeArch <- int64 <| snd l
                context.Archives.Add(arr) |> ignore
                context.SaveChanges() |> ignore
                ()
          ()
