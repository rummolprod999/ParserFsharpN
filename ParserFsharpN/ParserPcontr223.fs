namespace ParserFsharp

type ParserPcontr223(dir : string) =
      inherit AbstractParserFtpEis()
      interface Iparser with
      
            override this.Parsing() =
                  let regions = this.GetRegions()
                  
                  regions |> Seq.iter (fun x -> printfn "%s" x.Path223)
                  ()
