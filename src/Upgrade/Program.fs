// App to upgrade storage
open System.IO
open Lizard
open MBrace.FsPickler.Json


[<EntryPoint>]
let main argv = 
    //get old storage
    let opts = Opts.load()
    
    let wfol = 
        let ans = Path.Combine(opts.Opnfol, "White")
        ans
    let bfol = 
        let ans = Path.Combine(opts.Opnfol, "Black")
        ans
    
    let wfls = Directory.GetFiles(wfol, "*.pgn")
    let bfls = Directory.GetFiles(bfol, "*.pgn")
    //write new storage
    for pfn in wfls do
        let nm = Path.GetFileNameWithoutExtension(pfn)
        let pgns = PGN.ReadFromFile pfn
        let brchs = pgns |> List.map (fun g -> g.Moves)
        let vn = 
            { Name = nm
              Isw = true
              Brchs = brchs }
        let newfile = Path.Combine(wfol, nm + ".json")
        let json = FsPickler.CreateJsonSerializer()
        let str = json.PickleToString<Varn>(vn)
        File.WriteAllText(newfile, str)
    for pfn in bfls do
        let nm = Path.GetFileNameWithoutExtension(pfn)
        let pgns = PGN.ReadFromFile pfn
        let brchs = pgns |> List.map (fun g -> g.Moves)
        let vn = 
            { Name = nm
              Isw = false
              Brchs = brchs }
        let newfile = Path.Combine(bfol, nm + ".json")
        let json = FsPickler.CreateJsonSerializer()
        let str = json.PickleToString<Varn>(vn)
        File.WriteAllText(newfile, str)

    0 // return an integer exit code
