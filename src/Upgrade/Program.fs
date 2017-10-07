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
    
    let wfls = Directory.GetFiles(wfol, "*.json")
    let bfls = Directory.GetFiles(bfol, "*.json")
    //write new storage
    let json = FsPickler.CreateJsonSerializer()
    let mv2mv1 (m:Move):Move1 = 
        { Mfrom = m.Mfrom
          Mto = m.Mto
          Mtyp = m.Mtyp
          Mpgn = m.Mpgn
          Meval = Normal
          Scr10 = 0
          Scr25 = 0 
          Bresp = ""
          ECO = ""
          FicsPc = 0.0}


    for pfn in wfls do
        let nm = Path.GetFileNameWithoutExtension(pfn)
        File.Copy(pfn,pfn+".old",true)
        let str = File.ReadAllText(pfn)
        let vn = json.UnPickleOfString<Varn1>(str)
        let brchs1 = vn.Brchs|>List.map(fun l -> {Mvs=(l.Mvs|>List.map mv2mv1)})
        let vn1:Varn = {Name=vn.Name;Isw=vn.Isw;Brchs=brchs1}
        let str = json.PickleToString<Varn>(vn1)
        File.WriteAllText(pfn, str)
    for pfn in bfls do
        let nm = Path.GetFileNameWithoutExtension(pfn)
        File.Copy(pfn,pfn+".old",true)
        let str = File.ReadAllText(pfn)
        let vn = json.UnPickleOfString<Varn1>(str)
        let brchs1 = vn.Brchs|>List.map(fun l -> {Mvs=(l.Mvs|>List.map mv2mv1)})
        let vn1:Varn = {Name=vn.Name;Isw=vn.Isw;Brchs=brchs1}
        let str = json.PickleToString<Varn>(vn1)
        File.WriteAllText(pfn, str)

    0 // return an integer exit code
