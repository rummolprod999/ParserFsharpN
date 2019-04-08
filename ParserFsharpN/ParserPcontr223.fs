namespace ParserFsharp
open System
open Microsoft.FSharp.Data
open System.Linq
open System.Collections.Generic
open System.IO
open System.Linq
open Microsoft.EntityFrameworkCore
open System.Text

type ParserPcontr223(dir : string) =
      inherit AbstractParserFtpEis()
      interface Iparser with

            override __.Parsing() =
                  let regions = __.GetRegions()

                  for r in regions do
                       let mutable arr = new List<string * uint64>()
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

      member private __.Revision(f : FileInfo, pathParse : string, reg : Region) =
            match f with
            | x when (not (f.Name.ToLower().EndsWith(".xml"))) || f.Length = 0L -> ()
            | _ -> use sr = new StreamReader(f.FullName, Encoding.Default)
                   let str = __.DeleteBadSymbols(sr.ReadToEnd())
                   __.ParsingXml(str, reg)
            ()
      member private __.ParsingXml(s : string, reg : Region) =
            printfn "%s" s
            ()
      member private __.GetArrLastFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F223)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let searchStr = seq { for s in yearsSeq do yield sprintf "_%s_%d" region s }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            ret.ToList()

      member private __.GetArrCurrFromFtp(pathParse : string, region : string) =
            let arch = __.GetListArrays(pathParse, S.F223)
            let yearsSeq = seq { 2015..DateTime.Now.Year }
            let searchStr = seq { for s in yearsSeq do yield sprintf "_%s_%d" region s }
            let ret = query { for a in arch do
                              where (yearsSeq.Any(fun x -> (fst a).Contains(x.ToString())))
                              select a }
            use context = new ArchivePContr223Context()
            let arr = new List<string * uint64>()
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
