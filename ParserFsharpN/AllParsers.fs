namespace ParserFsharp

module P =

    let parserExec (p : Iparser) = p.Parsing()

    let parserPContr (dir : string) =
        Logging.Log.logger "Начало парсинга"
        try
            parserExec (ParserPcontr223(dir))
        with ex -> Logging.Log.logger ex
        Logging.Log.logger (sprintf "Добавили контрактов %d" AbstractDocumentFtpEis.Add)
        Logging.Log.logger (sprintf "Обновили контрактов %d" AbstractDocumentFtpEis.Upd)
        ()

