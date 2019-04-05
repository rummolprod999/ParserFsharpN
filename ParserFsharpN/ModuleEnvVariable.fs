namespace ParserFsharp

module S =
    type T =
        { TmpP : string
          Pref : string
          ConS : string }
    let mutable argTuple = Argument.None
    let mutable logFile = ""
    let mutable Settings = {
        TmpP = ""
        Pref = ""
        ConS = ""
    }
