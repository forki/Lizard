namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("LizardChess")>]
[<assembly: AssemblyProductAttribute("LizardChess")>]
[<assembly: AssemblyDescriptionAttribute("Chess Opening Tool written with F#.")>]
[<assembly: AssemblyVersionAttribute("0.1.0")>]
[<assembly: AssemblyFileVersionAttribute("0.1.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "0.1.0"