namespace Lizard

open System
open System.IO

module Varn = 
    ///cur - create Current Varn
    let cur (nm, isw) = 
        { Name = nm
          Isw = isw
          Brchs = [] }
    
    let emp = 
        { Name = "NotSet"
          Isw = true
          Brchs = [] }
    
    ///findsv - finds the selvar given posn and set of posns
    let findsv (mb : Move list) (curbs : Move list list) = 
        //function to find match
        let mtch (mvl : Move list) = mvl.Length >= mb.Length && mvl.[0..mb.Length - 1] = mb
        curbs |> List.tryFindIndex mtch
    
    ///findnmvs - finds the next moves given posn and set of posns
    let findnmvs (mb : Move list) (curbs : Move list list) = 
        //function to find match with extra move
        let mtch (mvl : Move list) = mvl.Length > mb.Length && mvl.[0..mb.Length - 1] = mb
        curbs
        |> List.filter mtch
        |> List.map (fun curb -> (curb |> List.rev).[mb.Length])
        |> Set.ofList
        |> Set.toList
    
    //function to find length same for two move list
    let rec smmv m1 m2 no = 
        if List.isEmpty m1 || List.isEmpty m2 || m1.Head <> m2.Head then no
        else smmv m1.Tail m2.Tail (no + 1)
    
    //function to find index of best match
    let rec fndidx rear cidx no idx mb = 
        if List.isEmpty rear then idx
        else 
            let curb = List.rev (rear.Head)
            let cno = smmv curb (List.rev mb) 0
            
            let nidx = 
                if cno < no then idx
                else cidx
            
            let nno = 
                if cno < no then no
                else cno
            
            fndidx rear.Tail (cidx + 1) nno nidx mb
    
    ///mrgbrch - merges a new branch into a list of branches
    let mrgbrch (mb : Move list) (curbs : Move list list) = 
        // either same as existing branch and then either do nothing or replace
        // or extra branch and need to put next to the nearest
        let mtchb = mb.[0..mb.Length - 2]
        
        //function to add to existing branch
        let rec addex front rear fnd = 
            if List.isEmpty rear then front, fnd
            else 
                let curb = rear.Head |> List.rev
                if curb.Length < mtchb.Length && curb = mtchb.[0..curb.Length - 1] then 
                    (front @ mb :: rear.Tail), true
                elif curb.Length >= mtchb.Length && curb.[0..mtchb.Length - 1] = mtchb then 
                    if mb.[mb.Length - 1] = curb.[mb.Length - 1] then (front @ rear), true
                    else (front @ mb :: rear.Tail), true
                else addex (front @ [ rear.Head ]) rear.Tail false
        
        // use functions
        let ans, fnd = addex [] curbs false
        if fnd then ans
        else 
            //need to find nearest branch
            let nidx = fndidx curbs 0 0 0 mb
            let frnt = curbs.[0..nidx]
            
            let rear = 
                if nidx < curbs.Length - 1 then curbs.[nidx + 1..curbs.Length - 1]
                else []
            frnt @ [ mb ] @ rear
    
    ///add - updates the variation with moves from a game move history
    let add (cur : Varn) (mvl : Move list) = 
        //don't add if wrong colour
        if mvl.Length = 0 then cur
        elif List.isEmpty cur.Brchs then { cur with Brchs = [ mvl ] }
        else { cur with Brchs = mrgbrch mvl cur.Brchs }
    
    ///del - deletes a line from the variation given index of branch to delete
    let del cur (sel : int) = 
        let brs = cur.Brchs
        let abrs = brs |> List.toArray
        
        let nbrs1 = 
            if sel = 0 then []
            else (List.ofArray abrs.[0..sel - 1])
        
        let nbrs2 = 
            if sel = abrs.Length - 1 then []
            else (List.ofArray abrs.[sel + 1..abrs.Length - 1])
        
        { cur with Brchs = nbrs1 @ nbrs2 }
    
    ///mvl2lines - support function that converts a move list to lines
    let mvl2lines mvl = 
        let rec genml ml (mh : Move list) isw = 
            if List.isEmpty mh then List.rev ml
            else if isw then genml ([| mh.Head.Mpgn; "" |] :: ml) mh.Tail (not isw)
            else 
                genml ([| ml.Head.[0]
                          mh.Head.Mpgn |]
                       :: ml.Tail) mh.Tail (not isw)
        genml [] mvl true |> List.toArray
    
    ///mvll2lines - support function that converts a move list list to lines
    let mvll2lines mvll = 
        let linesl = mvll |> List.map mvl2lines
        //find max lines length
        let maxl = ref 0
        linesl |> List.iter (fun lines -> 
                      if lines.Length > !maxl then maxl := lines.Length)
        //grow all lines to maxl
        let gline (l : string [] []) = 
            let ans = Array.create !maxl [| ""; "" |]
            for i = 0 to l.Length - 1 do
                ans.[i] <- l.[i]
            ans
        
        let glinesl = linesl |> List.map gline
        //merge two line in the list
        let mline = Array.map2 (Array.append)
        List.reduce mline glinesl
    
    ///lines - gets an array of lines for display
    let lines cur = 
        if List.isEmpty cur.Brchs then [||]
        else mvll2lines (List.map (fun b -> List.rev (b)) cur.Brchs)
    
    ///mvl - gets a move list given a cur and the column and the row
    let mvl (cur, cl, rwi) = 
        let fmvl = List.rev (cur.Brchs.[cl])
        
        let rw = 
            if rwi < fmvl.Length then rwi
            else fmvl.Length - 1
        fmvl.[0..rw]
    
    //STORAGE elements
    //set up paths
    let opts = Opts.load()
    
    let wfol = 
        let ans = Path.Combine(opts.Opnfol, "White")
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    let bfol = 
        let ans = Path.Combine(opts.Opnfol, "Black")
        Directory.CreateDirectory(ans) |> ignore
        ans
    
    ///wvars gets list of white varns
    let wvars() = 
        Directory.GetFiles(wfol, "*.pgn")
        |> Array.map Path.GetFileNameWithoutExtension
        |> List.ofArray
    
    let wvarso() = wvars() |> List.map (fun s -> s :> obj)
    
    ///bvars gets list of black varns
    let bvars() = 
        Directory.GetFiles(bfol, "*.pgn")
        |> Array.map Path.GetFileNameWithoutExtension
        |> List.ofArray
    
    let bvarso() = bvars() |> List.map (fun s -> s :> obj)
    
    /// cur2txt - generates array of string of moves from cur varn
    let cur2txt cur = 
        cur.Brchs
        |> List.map (fun mvl -> mvl|>List.map(fun m -> m.Mpgn)|>List.reduce(fun a b -> a + " " + b))
        |> List.toArray
    
    ///save - serializes the varn to a file in binary
    let save (cur : Varn) = 
        try 
            let pfn = 
                Path.Combine((if cur.Isw then wfol
                              else bfol), cur.Name + ".pgn")
            
            let nl = Environment.NewLine
            
            let pstr = 
                cur.Brchs
                |> List.map (fun mvl -> mvl|>List.map(fun m -> m.Mpgn)|>List.reduce(fun a b -> a + " " + b))
                |> List.reduce (fun a b -> a + nl + b)
            File.WriteAllText(pfn, pstr)
            "Save successful for variation: " + cur.Name
        with e -> "Save failed with error: " + e.ToString()
    
    ///saveas - saves the cur with a new name and returns the renamed cur
    let saveas (cur, nm) = 
        let ans = { cur with Name = nm }
        save ans |> ignore
        ans
    
    ///load - deserializes to a varn from a file
    let load (nm, isw) = 
        //set this to file in White folder with filename same as name
        let pfn = 
            Path.Combine((if isw then wfol
                          else bfol), nm + ".pgn")
        
        let pgns = PGN.ReadFromFile pfn
        //TODO
        let brchs = pgns |> List.map (fun g -> g.Moves)
        { Name = nm
          Isw = isw
          Brchs = brchs }
    
    ///load - deserializes to a varn from a file or files 
    let loada (nm, isw) = 
        let vnnames = 
            if nm = "<All>" then 
                (if isw then wvars()
                 else bvars())
            else [ nm ]
        
        let brs = 
            vnnames
            |> List.map (fun nm -> (load (nm, isw)).Brchs)
            |> List.concat
        
        { Name = nm
          Isw = isw
          Brchs = brs }
    
    ///load - deserializes to a text array from a file
    let loadtxt (nm, isw) = 
        let cur = load (nm, isw)
        cur2txt cur
    
    ///load - deserializes to a varn from a file or files 
    let loadtxta (nm, isw) = 
        let vnnames = 
            if nm = "<All>" then 
                (if isw then wvars()
                 else bvars())
            else [ nm ]
        vnnames
        |> List.map (fun nm -> loadtxt (nm, isw))
        |> Array.concat
    
    ///delete - deletes the variation file 
    let delete (nm, isw) = 
        File.Delete(Path.Combine((if isw then wfol
                                  else bfol), nm + ".json"))
        File.Delete(Path.Combine((if isw then wfol
                                  else bfol), nm + ".pgn"))
