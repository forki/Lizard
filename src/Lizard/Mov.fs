namespace LizardChess

module Mov = 
    let create (mvpcimg, from, mto, sqcap, mvname, fiftycntr, pgn, uci) = 
        { MvPcImg = mvpcimg
          From = from
          To = mto
          SqCap = sqcap
          MvName = mvname
          FiftyCntr = fiftycntr
          PGN = pgn
          UCI = uci }

    let PgnPart1 mv dup = 
        if mv.MvName = CastleK then "O-O"
        elif mv.MvName = CastleQ then "O-O-O"
        else 
            let strpc = 
                if mv.MvPcImg % 6 <> 0 then 
                    (Piece.letr mv.MvPcImg).ToUpper()
                else ""
                
            let strfrom = Ref.sq.[mv.From]
                
            let strcap = 
                if (mv.SqCap <> -1) then "x"
                else ""
                
            let strto = Ref.sq.[mv.To]
                
            let strfrom2 = 
                if dup <> "" 
                    && dup.Substring(0, 1) = strfrom.Substring(0, 1) then 
                    strfrom.Substring(1, 1)
                elif dup <> "" then strfrom.Substring(0, 1)
                elif mv.MvPcImg % 6 = 0 && strcap = "x" then 
                    strfrom.Substring(0, 1)
                else ""
            strpc + strfrom2 + strcap + strto
    
    let PGN(mv, dup, incheckmate, incheck) = 
        let part1 = PgnPart1 mv dup 
        let part2 = 
            match mv.MvName with
            | PromQ -> "=Q"
            | PromR -> "=R"
            | PromN -> "=N"
            | PromB -> "=B"
            | _ -> ""
        
        let part3 = 
            if incheckmate then "#"
            elif incheck then "+"
            else ""
        
        part1 + part2 + part3
    
    /// coord - suppport function to convert move parts to string
    let coord sq = Ref.f.[sq] + Ref.r.[sq]
    
    /// uci - suppport function to convert move to uci string
    let uci mv = 
        (coord mv.From) + (coord mv.To) + (if mv.MvName = OMoveName.PromB then 
                                               "b"
                                           elif mv.MvName = OMoveName.PromN then 
                                               "n"
                                           elif mv.MvName = OMoveName.PromR then 
                                               "r"
                                           elif mv.MvName = OMoveName.PromQ then 
                                               "q"
                                           else "")
    
    /// fromuci - suppport function to convert uci string to move bits
    let fromuci (c : string) = 
        let f = Ref.ord (c.[0], c.[1])
        let t = Ref.ord (c.[2], c.[3])
        
        let mn = 
            if c.Length = 4 then OMoveName.NullMove
            elif c.[4] = 'b' then OMoveName.PromB
            elif c.[4] = 'n' then OMoveName.PromN
            elif c.[4] = 'r' then OMoveName.PromR
            else OMoveName.PromQ
        f, t, mn