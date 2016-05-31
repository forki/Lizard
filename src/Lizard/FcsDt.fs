namespace LizardChess

open System
open HttpClient
open System.IO
open System.Xml.Linq
open Nessos.FsPickler.Json

module FcsDt = 
    //STORAGE elements
    //set up paths
    let opts = Opts.load()
    let json = FsPickler.CreateJsonSerializer()
    
    let ofol = 
        let ans = opts.Opnfol
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    let ficsdbstor = Path.Combine(ofol, "FicsDbStore.json")
    
    ///let loadFicsDbStore - loads dictionary of Fics DB results
    let loadFicsDbStore() = 
        if (File.Exists ficsdbstor) then 
            let str = File.ReadAllText(ficsdbstor)
            json.UnPickleOfString<Fdbstr>(str)
        else new Fdbstr()
    
    ///let saveFicsDbStore - saves dictionary of Fics DB results
    let saveFicsDbStore (fs : Fdbstr) = 
        let str = json.PickleToString<Fdbstr>(fs)
        File.WriteAllText(ficsdbstor, str)
    
    /// empanl - empty enganl
    let empfdb = 
        { FENlong = ""
          ECO = ""
          ECOName = ""
          NumGames = 0
          MvList = [||] }
    
    /// getfdb - gets fisc db data given string of moves
    let getfdb (fs : Fdbstr) k = 
        let p, v = fs.TryGetValue k
        if p then v
        else empfdb
    
    /// get XML from Fics given FEN
    let getxml fen = 
        let site = @"http://www.ficsgames.org/cgi-bin/explorer.cgi"
        
        let resp = 
            createRequest Get site
            |> withQueryStringItem { name = "FEN"
                                     value = fen }
            |> getResponseBody
        resp
    
    ///Convert XML to ficsdata type
    let xml2ficsd x = 
        let xns s = XName.Get(s)
        let xml = XDocument.Parse x
        
        let getit str = 
            let a = 
                xml.Descendants(xns str)
                |> Seq.map (fun e -> e.Value)
                |> Seq.toArray
            if a.Length > 0 then a.[0]
            else ""
        
        let bits = 
            [| "FENlong"; "ECO"; "ECOName"; "NumGames" |] |> Array.map getit
        
        let mvs = 
            let getmv (m : XElement) = 
                let gi s = 
                    m.Descendants(xns s)
                    |> Seq.map (fun e -> e.Value)
                    |> Seq.head
                { Fpgn = gi "SAN"
                  Wpc = 100.0 * float (gi "ww") / float (gi "n")
                  Dpc = 100.0 * float (gi "d") / float (gi "n")
                  Bpc = 100.0 * float (gi "bw") / float (gi "n")
                  Fnum = int (gi "n") }
            
            let mvl = xml.Descendants(xns "MvList")
            mvl.Descendants(xns "Mv")
            |> Seq.toArray
            |> Array.map getmv
            |> Array.filter (fun m -> m.Fnum > 0)
        { FENlong = bits.[0]
          ECO = bits.[1]
          ECOName = bits.[2]
          NumGames = int (bits.[3])
          MvList = mvs }
    
    ///Convert posn to ficsdata type
    let pos2ficsd p = 
        let ls = Posn.psn2str p
        let fs = loadFicsDbStore()
        if not (fs.ContainsKey(ls)) then
            let fen = Posn.ToFen p
            try 
                let fd = 
                    fen
                    |> getxml
                    |> xml2ficsd
                fs.[ls] <- fd
                saveFicsDbStore (fs)
            with e -> failwith ("Fics Data failed: " + e.Message + " for " + fen)
    
    ///Convert posn to pos * ficsdata type list
    let pos2ficsdlst p = 
        let rec loadp cp mvl = 
            if mvl <> [] then 
                let np = Posn.DoMove(mvl.Head, cp)
                pos2ficsd np
                loadp np (mvl.Tail)
        loadp Posn.st (p.Mhst |> List.rev)