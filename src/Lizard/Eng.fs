namespace Lizard

open System
open System.IO
open MBrace.FsPickler.Json

module Eng = 
    //STORAGE elements
    //set up paths
    let opts = Opts.load()
    let json = FsPickler.CreateJsonSerializer()
    
    let efol = 
        let ans = opts.Engfol
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    let logfile = Path.Combine(efol, "LogFile.txt")
    let linestor = Path.Combine(efol, "LineStore.json")
    
    ///let loadLineStore - loads dictionary of line analysis results
    let loadLineStore() = 
        if (File.Exists linestor) then 
            let str = File.ReadAllText(linestor)
            json.UnPickleOfString<Linstr>(str)
        else new Linstr()
    
    ///let saveLineStore - saves dictionary of line analysis results
    let saveLineStore (ls : Linstr) = 
        let str = json.PickleToString<Linstr>(ls)
        File.WriteAllText(linestor, str)
    
    /// empanl - empty enganl
    let empanl = 
        { Depth = 0
          Scr = 0
          Bestmv = ""
          Resp = ""
          BmPGN = ""
          RmPGN = "" }
    
    /// getanl - gets engine analysis given string of moves
    let getanl (ls : Linstr) k = 
        let p, v = ls.TryGetValue k
        if p then 
            let bits = v.Split([| ',' |])
            { Depth = int (bits.[0])
              Scr = int (bits.[1])
              Bestmv = bits.[2]
              Resp = bits.[3]
              BmPGN = bits.[4]
              RmPGN = bits.[5] }
        else empanl
    
    /// getanls - gets engine analyses given varn
    let getanls (ls : Linstr, (vn : Varn)) = 
        let strs = Varn.cur2txt vn
        strs |> Array.map (getanl ls)
    
    /// dpth - gets dpth from string from line store
    let dpth (v : string) = 
        let bits = v.Split([| ',' |])
        int (bits.[0])
    
    /// alreadyDone - confirms whether already in line store
    let alreadyDone (ls : Linstr, k, d) = 
        let p, curv = ls.TryGetValue k
        (p && (dpth curv) >= d)
    
    /// addLineStore - adds to dictionary of line analysis results
    let addLineStore (ls : Linstr, k, v) = 
        if not (alreadyDone (ls, k, dpth v)) then ls.[k] <- v
    
    //Utility with array of mvs
    ///let mvs2str - converts an array of mvs to string given a length
    let mvs2str (mvs : string [], ct) = 
        if ct = 0 then ""
        else Array.reduce (fun a b -> a + " " + b) mvs.[0..ct - 1]
    
    ///let str2mvs - converts string to an array of mvs given a length
    let str2mvs (str : string, ct) = 
        if ct = 0 then [||]
        else 
            let arr = str.Split([| ' ' |])
            arr.[0..ct - 1]
    
    ///let str2str - converts string to a string given a length
    let str2str (str : string, ct) = mvs2str (str2mvs (str, ct), ct)
    
    ///let str2psn - converts string to a psn
    let str2psn (str : string) = 
        let strmvsl = str.Split([| ' ' |]) |> List.ofArray
        
        let rec genpsn (psn : Pos) strl = 
            if List.isEmpty strl then psn
            else 
                let mv = psn.GetMvUCI(strl.Head)
                psn.DoMv mv
                genpsn psn strl.Tail
        genpsn (Pos.Start()) strmvsl
    
    ///let procbm - processes best move information from answer from engine and stores result
    let procbm (ls : Linstr, ln, mvct, answer : string, scrlins : string list) = 
        let getitem (lin : string) lbl = 
            let rec getit (lst : string list) = 
                let hd = lst.Head
                if List.isEmpty lst.Tail then ""
                elif hd = lbl then lst.Tail.Head
                else getit lst.Tail
            lin.Split([| ' ' |])
            |> List.ofArray
            |> getit
        
        let cplines = scrlins |> List.filter (fun l -> l.Contains("cp "))
        let bmlines = scrlins |> List.filter (fun l -> l.Contains("bestmove "))
        if cplines.Length >= 1 && bmlines.Length = 1 then 
            let cpline = cplines.Head
            let bmline = bmlines.Head
            if answer.Length > 3 then 
                let scr = getitem cpline "cp"
                let depth = getitem cpline "depth"
                let resp = getitem cpline answer
                if not (scr = "" || depth = "") && resp.Length > 3 then 
                    let strmvs = str2str (ln, mvct)
                    let psn = str2psn strmvs
                    let bm = psn.GetMvUCI answer
                    psn.DoMv bm
                    let bmPgn = bm.Mpgn
                    let rm = psn.GetMvUCI resp
                    psn.DoMv rm
                    let rmPgn = rm.Mpgn
                    addLineStore (ls, strmvs, depth + "," + scr + "," + answer + "," + resp + "," + bmPgn + "," + rmPgn)
    
    //general engine analysis for a variation
    let rec getbms (bms : Engbm list) (len, bw, num, cpsn) (mvl : Move list) (str : string) (ls : Linstr) = 
        if len >= str.Length - 5 then List.rev bms
        else 
            let curstr = str.Substring(0, len)
            let curmv = str.Substring(len + 1, 4)
            let anl = getanl ls curstr
            let nlen = len + 5
            
            let nbw = 
                if bw = "B" then "W"
                else "B"
            
            let nnum = 
                if bw = "B" then num + 1
                else num
            
            // need to make a move and get the PGN
            let bmPgn = anl.BmPGN
            let bscr = anl.Scr
            //TODO
            //let opsn = Posn.DoMove(mvl.Head, cpsn)
            if anl.Bestmv = curmv || anl.Bestmv = "" then getbms bms (nlen, nbw, nnum, []) mvl.Tail str ls
            else 
                getbms ({ Bnum = num
                          Bisw = 
                              (if bw = "W" then true
                               else false)
                          BPGN = bmPgn
                          Bstr = (bw + num.ToString() + "-" + bmPgn)
                          Bscr = bscr }
                        :: bms) (nlen, nbw, nnum, []) mvl.Tail str ls
    
    let strmvl2bms (ls : Linstr) (str : string) (mvl : Move list) = 
        //        let mvl = List.rev vnpsn.Mhst
        //        let opsn = Posn.DoMove(mvl.Head, Posn.st)
        let lbms = getbms [] (4, "B", 1, []) mvl.Tail str ls
        lbms |> List.toArray
    
    /// extlin - extends line given varn, enganls and selvar
    let extlin ((vn : Varn), (ea : Enganl []), (s : int)) = 
        let barr = vn.Brchs |> List.toArray
        let selb = barr.[s]
        let sele = ea.[s]
        { //TODO
          //        if not (sele.Bestmv = "" || sele.Resp = "") then 
          //            let bm = Posn.FndMv(sele.Bestmv, selb)
          //            let p1 = Posn.DoMove(bm.Value, selb)
          //            let rm = Posn.FndMv(sele.Resp, p1)
          //            let p2 = Posn.DoMove(rm.Value, p1)
          //            barr.[s] <- p2
          vn with Brchs = List.ofArray (barr) }
    
    /// extall - extends all lines given varn, enganls
    let extall ((vn : Varn), (ea : Enganl [])) = 
        let barr = vn.Brchs |> List.toArray
        for s = 0 to barr.Length - 1 do
            let selb = barr.[s]
            let sele = ea.[s]
            ()
        { //TODO
          //            if not (sele.Bestmv = "" || sele.Resp = "") then 
          //                let bm = Posn.FndMv(sele.Bestmv, selb)
          //                let p1 = Posn.DoMove(bm.Value, selb)
          //                let rm = Posn.FndMv(sele.Resp, p1)
          //                let p2 = Posn.DoMove(rm.Value, p1)
          //                barr.[s] <- p2
          vn with Brchs = List.ofArray (barr) }
    
    ///get header for curent anal
    let hdr (eng, ln, lnct, mvct, dpth) = 
        let mutable msg = "Analysing using " + eng
        msg <- msg + " - line " + ln
        msg <- msg + " - move  " + mvct.ToString()
        msg <- msg + " of " + (lnct+1).ToString()
        msg <- msg + " to depth " + dpth.ToString()
        msg
