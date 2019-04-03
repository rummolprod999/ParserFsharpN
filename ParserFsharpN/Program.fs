// Learn more about F# at http://fsharp.org
namespace ParserFsharp
open System
        
module EntryPoint =
        let arguments = "pcontr223 last, pcontr223 daily"    
        [<EntryPoint>]
        let main argv =
            if argv.Length = 0 then
                printf "Bad arguments, use %s" arguments
                Environment.Exit(1)
            0 

