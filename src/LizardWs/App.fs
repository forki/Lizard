open Suave
open LizardWs

[<EntryPoint>]
let main argv =
    let openingsWebPart = choose[restchess "wvars";restchess "bvars"]
    let app = choose[openingsWebPart]
    startWebServer defaultConfig app
    0