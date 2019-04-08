namespace ParserFsharp
open System.Collections.Generic
open System.Linq
open Microsoft.EntityFrameworkCore
open FluentFTP
open System.Threading

[<AbstractClass>]
type AbstractParserFtpEis() =
    abstract GetRegions : unit -> IEnumerable<Region>
    override this.GetRegions() =
        use context = new RegionContext()
        let regions : IEnumerable<Region> = context.Regions.AsNoTracking().ToList() :> IEnumerable<Region>
        regions

    member __.GetListArrays(pathParse : string, s : S.FtpUser) =
        let mutable arch = new List<string * uint64>()
        let mutable count = 1
        let mutable wh = true
        while wh do
            try
                let ftp = new FtpClient("ftp.zakupki.gov.ru", s.User, s.Pass)
                ftp.SetWorkingDirectory(pathParse)
                let ltmp = ftp.GetListing()
                ltmp |> Seq.iter (fun x -> arch.Add(x.Name, (uint64) x.Size))
                wh <- false
                if count > 1 then Logging.Log.logger (sprintf "Удалось получить список архивов после попытки %d" count)
            with
                | ex when ex.Message.Contains("Failed to change directory") -> Logging.Log.logger (sprintf "Не смогли найти директорию %s" pathParse)
                                                                               wh <- false
                | e when count > 3 -> Logging.Log.logger (sprintf "Не смогли найти директорию после попытки %d" count)
                                      Logging.Log.logger e
                                      wh <- false
                | _ -> count <- count + 1
                       Thread.Sleep(2000)
            ()
        arch
