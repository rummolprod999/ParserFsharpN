namespace ParserFsharp
open System
open Microsoft.FSharp.Data
open System.Linq
open System.Collections.Generic
open System.Linq
open Microsoft.EntityFrameworkCore

type ParserPcontr223(dir : string) =
      inherit AbstractParserFtpEis()
      interface Iparser with

            override __.Parsing() =
                  let regions = __.GetRegions()

                  for r in regions do
                       let mutable arr = new List<string * uint64>()
                       match dir with
                       | "last" -> arr <- __.GetArrLastFromFtp ((sprintf "/out/published/%s/purchaseContract" r.Path223), r.Path223)
                       | "curr" -> arr <- __.GetArrCurrFromFtp ((sprintf "/out/published/%s/purchaseContract/daily" r.Path223), r.Path223)
                       | _ -> ()
                       arr |> Seq.iter (fun n -> printfn "%s %d" (fst n) (snd n))
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
                  | _ -> let res = context.Archives.Where(fun x -> x.Archive = (fst r) && (uint64 x.SizeArch = (snd r) || uint64 x.SizeArch = 0UL)) .Count()
                         if res = 0 then arr.Add(r)
                         ()
            ret.ToList()

      member private __.WriteArrToTale() =
          ()
