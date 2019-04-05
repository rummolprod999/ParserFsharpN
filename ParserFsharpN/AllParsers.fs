namespace ParserFsharp

module P =
    let stn = match !Stn.Settings with
                  | Stn.None ->
                    Logging.Log.logger "bad settings"
                    failwith "bad settings"
                  | Stn.St x -> x
    
    let parserExec (p: Iparser) = p.Parsing()
    
    let parserPContr (dir : string) =
        Logging.Log.logger "Начало парсинга"
        try
            parserExec(ParserPcontr223())
        with ex -> Logging.Log.logger ex
        Logging.Log.logger (sprintf "Добавили контрактов %d" AbstractDocumentFtpEis.Add)
        Logging.Log.logger (sprintf "Обновили контрактов %d" AbstractDocumentFtpEis.Upd)
        ()

