namespace Lizard

open System
open System.IO
open Nessos.FsPickler.Json

module Test = 
    //STORAGE elements
    //set up paths
    let opts = Opts.load()
    let json = FsPickler.CreateJsonSerializer()
    
    let tfol = 
        let ans = opts.Tstfol
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    let resfil = Path.Combine(tfol, "Results.json")
    let resfillin = Path.Combine(tfol, "ResultsLin.json")
    
    ///loadResults - get stored results
    let loadResults() = 
        if (File.Exists resfil) then 
            let str = File.ReadAllText(resfil)
            json.UnPickleOfString<Tstres []>(str)
        else [||]
    
    ///loadResultsLin - get stored results
    let loadResultsLin() = 
        if (File.Exists resfillin) then 
            let str = File.ReadAllText(resfillin)
            json.UnPickleOfString<Tstres []>(str)
        else [||]
    
    ///saveResults - save results to file
    let saveResults (resa) = 
        let str = json.PickleToString<Tstres []>(resa)
        File.WriteAllText(resfil, str)
    
    ///saveResultsLin - save results to file
    let saveResultsLin (resa) = 
        let str = json.PickleToString<Tstres []>(resa)
        File.WriteAllText(resfillin, str)
    
    //saveRes - save results given both an array of results and a result
    let saveRes (resarr, res) = 
        //need to first merge the res in
        let rec mrgres frl rrl r = 
            if List.isEmpty frl then 
                (r :: rrl)
                |> List.rev
                |> List.toArray
            else 
                let cr = frl.Head
                if cr.Vname = r.Vname && cr.Visw = r.Visw then (List.rev rrl) 
                                                               @ (r :: frl.Tail)
                                                               |> List.toArray
                else mrgres frl.Tail (cr :: rrl) r
        
        let mrg = mrgres (resarr |> List.ofArray) [] res
        mrg |> saveResults
    
    //saveRes - save results given both an array of results and a result
    let saveResLin (resarr, res) = 
        //need to first merge the res in
        let rec mrgres frl rrl r = 
            if List.isEmpty frl then 
                (r :: rrl)
                |> List.rev
                |> List.toArray
            else 
                let cr = frl.Head
                if cr.Vname = r.Vname && cr.Visw = r.Visw then (List.rev rrl) 
                                                               @ (r :: frl.Tail)
                                                               |> List.toArray
                else mrgres frl.Tail (cr :: rrl) r
        
        let mrg = mrgres (resarr |> List.ofArray) [] res
        mrg |> saveResultsLin
    
    //createres - create a res given name, isw and res
    let createres (nm, isw, rs) = 
        { Vname = nm
          Visw = isw
          Dte = DateTime.Now
          Res = rs }
    
    //getallres - gets an array of an array of all results
    let getallres() = 
        let reslst = loadResults() |> List.ofArray
        let wvars = Lizard.Varn.wvars()
        let bvars = Lizard.Varn.bvars()
        
        let rec getarr isw nm resl = 
            if List.isEmpty resl then 
                [| (if isw then "White"
                    else "Black")
                   nm
                   "N/A"
                   "-" |]
            else 
                let res = resl.Head
                if (res.Vname = nm && res.Visw = isw) then 
                    [| (if isw then "White"
                        else "Black")
                       nm
                       res.Dte.ToLongDateString()
                       res.Res.ToString() |]
                else getarr isw nm resl.Tail
        
        let wl = wvars |> List.map (fun nm -> getarr true nm reslst)
        let bl = bvars |> List.map (fun nm -> getarr false nm reslst)
        (wl @ bl) |> List.toArray
    
    //getallres - gets an array of an array of all results
    let getallreslin() = 
        let reslst = loadResultsLin() |> List.ofArray
        let wvars = Lizard.Varn.wvars()
        let bvars = Lizard.Varn.bvars()
        
        let rec getarr isw nm resl = 
            if List.isEmpty resl then 
                [| (if isw then "White"
                    else "Black")
                   nm
                   "N/A"
                   "-" |]
            else 
                let res = resl.Head
                if (res.Vname = nm && res.Visw = isw) then 
                    [| (if isw then "White"
                        else "Black")
                       nm
                       res.Dte.ToLongDateString()
                       res.Res.ToString() |]
                else getarr isw nm resl.Tail
        
        let wl = wvars |> List.map (fun nm -> getarr true nm reslst)
        let bl = bvars |> List.map (fun nm -> getarr false nm reslst)
        (wl @ bl) |> List.toArray
    
    // GENERAL FUNCTIONS
    let GetPosn tst = tst.Mvl|>Pos.FromMoves
    
    let rec ListIncl (tst : TestDet) tstlst = 
        if List.isEmpty tstlst then false
        elif tstlst.Head = tst then true
        else ListIncl tst tstlst.Tail
    
    /// ListAdd - only adds if not present
    let ListAdd tst tstlst = 
        if ListIncl tst tstlst then tstlst
        else tst :: tstlst
    
    //get a ful array of test positions from a line
    let rec fromLine (front:Lizard.Types.Move list) (rear:Lizard.Types.Move list) tests nm = 
        if List.isEmpty rear then tests
        elif List.isEmpty rear.Tail then 
            ListAdd { Mvl = front
                      Mv = rear.Head
                      Vnname = nm
                      Status = "Not Done" } tests
        else 
            let mv1 = rear.Head
            let mv2 = rear.Tail.Head
            fromLine (front @ [ mv1; mv2 ]) rear.Tail.Tail 
                (ListAdd { Mvl = front
                           Mv = mv1
                           Vnname = nm
                           Status = "Not Done" } tests) nm
    
    //get a full array of test positions from a variation for random tests
    let fromVarn (vn : Lizard.Types.Varn) = 
        let tstlst = 
            if vn.Isw then 
                vn.Brchs 
                |> List.map 
                       (fun b -> fromLine [] b [] (vn.Name))
            else 
                vn.Brchs 
                |> List.map 
                       (fun b -> 
                       fromLine [ b.Head ] 
                           b.Tail [] (vn.Name))
        tstlst
        |> List.concat
        |> List.toArray
    
    // fromName - generate set of test positions given name of variation and whether white
    let fromName vnname isw = 
        let vnnames = 
            if vnname = "<All>" then 
                if isw then Lizard.Varn.wvars()
                else Lizard.Varn.bvars()
            else [ vnname ]
        
        let vns = vnnames |> List.map (fun nm -> Lizard.Varn.load (nm, isw))
        
        let alltests = 
            vns
            |> List.map fromVarn
            |> List.toArray
            |> Array.concat
        
        //load settings
        let opts = Opts.load()
        //filter to remove early positions
        let alltestsfilt1 = 
            alltests |> Array.filter (fun t -> t.Mvl.Length > opts.Rskip * 2)
        
        //reset if no tests left
        let alltestsfilt = 
            if alltestsfilt1.Length = 0 then alltests
            else alltestsfilt1
        
        //select random subset
        let alltestgenl = 
            new System.Collections.Generic.List<TestDet>(alltestsfilt)
        
        let num = 
            if alltestsfilt.Length < opts.Rnum then alltestsfilt.Length
            else opts.Rnum
        
        // gets random set
        let ans = Array.zeroCreate num
        let rnd = System.Random()
        for i = 0 to num - 1 do
            let c = rnd.Next(alltestgenl.Count - 1)
            ans.[i] <- alltestgenl.[c]
            alltestgenl.RemoveAt(c)
        ans
    
    // fromNameLin - generate set of test positions given name of variation and whether white
    let fromNameLin vnname isw = 
        // get settings
        let opts = Opts.load()
        // get all lines
        let vn = Lizard.Varn.load (vnname, isw)
        
        let tstlst = 
            if vn.Isw then 
                vn.Brchs 
                |> List.map 
                       (fun b -> fromLine [] (List.rev b) [] (vn.Name))
            else 
                vn.Brchs 
                |> List.map 
                       (fun b -> 
                       fromLine [ (List.rev b).Head ] 
                           (List.rev b).Tail [] (vn.Name))
        
        //select random subset
        let alltestgenl = new System.Collections.Generic.List<TestDet list>(tstlst)
        
        let num = 
            if tstlst.Length < opts.Lnum then tstlst.Length
            else opts.Lnum
        
        // gets random set
        let tstlstsub = Array.zeroCreate num
        let rnd = System.Random()
        for i = 0 to num - 1 do
            let c = rnd.Next(alltestgenl.Count - 1)
            tstlstsub.[i] <- alltestgenl.[c]
            alltestgenl.RemoveAt(c)
        // reverse move list and concatenate
        let tstarr = 
            tstlstsub
            |> Array.map (fun l -> List.rev l)
            |> List.concat
            |> List.toArray
        
        //filter to remove early positions
        let tstarrfilt1 = 
            tstarr |> Array.filter (fun t -> t.Mvl.Length > opts.Lskip * 2)
        
        //reset if no tests left
        let tstarrfilt = 
            if tstarrfilt1.Length = 0 then tstarr
            else tstarrfilt1
        tstarrfilt