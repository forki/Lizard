// App to upgrade storage
open System.IO
open Lizard
open MBrace.FsPickler.Json


[<EntryPoint>]
let main argv = 
    //get old storage
    let opnfol = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Openings")
    
    let wfol = 
        let ans = Path.Combine(opnfol, "White")
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    let bfol = 
        let ans = Path.Combine(opnfol, "Black")
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    let wfls = Directory.GetFiles(wfol, "*.json")
    let bfls = Directory.GetFiles(bfol, "*.json")
    //write new storage
    let json = FsPickler.CreateJsonSerializer()
    let mv2mv1 (m:Move2):Move = 
        { Mfrom = m.Mfrom
          Mto = m.Mto
          Mtyp = m.Mtyp
          Mpgn = m.Mpgn
          Meval = m.Meval
          Scr10 = m.Scr10
          Scr25 = m.Scr25
          Bresp = m.Bresp
          ECO = m.ECO
          FicsPc = m.FicsPc}


    for pfn in wfls do
        let nm = Path.GetFileNameWithoutExtension(pfn)
        File.Copy(pfn,pfn+".old",true)
        let str = File.ReadAllText(pfn)
        let vn = json.UnPickleOfString<Varn2>(str)
        let brchs1:Line list = vn.Lines|>List.map(fun l -> {ECO=l.ECO;Mvs=(l.Mvs|>List.map mv2mv1)})
        let vn1:Varn = {Name=vn.Name;Isw=vn.Isw;ECO=vn.ECO;Lines=brchs1}
        let str = json.PickleToString<Varn>(vn1)
        File.WriteAllText(pfn, str)
    for pfn in bfls do
        let nm = Path.GetFileNameWithoutExtension(pfn)
        File.Copy(pfn,pfn+".old",true)
        let str = File.ReadAllText(pfn)
        let vn = json.UnPickleOfString<Varn2>(str)
        let brchs1:Line list = vn.Lines|>List.map(fun l -> {ECO=l.ECO;Mvs=(l.Mvs|>List.map mv2mv1)})
        let vn1:Varn = {Name=vn.Name;Isw=vn.Isw;ECO=vn.ECO;Lines=brchs1}
        let str = json.PickleToString<Varn>(vn1)
        File.WriteAllText(pfn, str)

    0 // return an integer exit code
