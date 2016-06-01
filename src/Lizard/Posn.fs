namespace LizardChess

open System
open System.Linq
open System.IO

module Posn = 
    let create (pcs, trn, mhst, iswhite) = 
        { Pcs = pcs
          Trn = trn
          Mhst = mhst
          IsWhite = iswhite }
    
    let st = 
        let pcs = Array.zeroCreate 32
        pcs.[0] <- Piece.create (0, 0, 0, 3)
        pcs.[1] <- Piece.create (1, 0, 0, 2)
        pcs.[2] <- Piece.create (2, 0, 0, 1)
        pcs.[3] <- Piece.create (3, 0, 0, 5)
        pcs.[4] <- Piece.create (4, 0, 0, 4)
        pcs.[5] <- Piece.create (5, 0, 0, 1)
        pcs.[6] <- Piece.create (6, 0, 0, 2)
        pcs.[7] <- Piece.create (7, 0, 0, 3)
        for i = 8 to 15 do
            pcs.[i] <- Piece.create (i, 0, 0, 0)
        pcs.[16] <- Piece.create (56, 0, 0, 9)
        pcs.[17] <- Piece.create (57, 0, 0, 8)
        pcs.[18] <- Piece.create (58, 0, 0, 7)
        pcs.[19] <- Piece.create (59, 0, 0, 11)
        pcs.[20] <- Piece.create (60, 0, 0, 10)
        pcs.[21] <- Piece.create (61, 0, 0, 7)
        pcs.[22] <- Piece.create (62, 0, 0, 8)
        pcs.[23] <- Piece.create (63, 0, 0, 9)
        for i = 24 to 31 do
            pcs.[i] <- Piece.create (i + 24, 0, 0, 6)
        create (pcs |> List.ofArray, 0, [], true)
    
    let stsqs = st.Sqrs
    
    let FenPart1 p = 
        let mutable ans = ""
        let mutable iNbrEmptySquare = 0
        for indRank = 7 downto 0 do
            if indRank <> 7 then ans <- ans + "/"
            for indFile = 0 to 7 do
                let ordThis = Ref.GetOrdFR(indFile, indRank)
                let pcl = p.Pcs |> List.filter (fun pc -> pc.Sq = ordThis)
                if pcl.Length = 0 then 
                    iNbrEmptySquare <- iNbrEmptySquare + 1
                else 
                    if (iNbrEmptySquare > 0) then 
                        ans <- ans + iNbrEmptySquare.ToString()
                        iNbrEmptySquare <- 0
                    ans <- ans + Piece.letr (pcl.[0].Img)
            if (iNbrEmptySquare > 0) then 
                ans <- ans + iNbrEmptySquare.ToString()
                iNbrEmptySquare <- 0
        ans
    let FenPart2 p = 
        if (p.Trn % 2 = 0) then " w "
        else " b "
    let FenPart3 (p:Posn) =
        let mutable ans = ""
        if p.CanK then ans <- "K"
        if p.CanQ then ans <- ans + "Q"
        if p.Cank then ans <- ans + "k"
        if p.Canq then ans <- ans + "q"
        if ans = "" then ans <- "-"
        ans
    let FenPart4 p =
        if p.Mhst.Length > 0 then 
            let lastMove = p.Mhst.[0]
            let sq = lastMove.To
            let lastMovepc = 
                (p.Pcs |> List.filter (fun pc -> pc.Sq = sq)).[0]
            let firstPmove = 
                (((Ref.ri.[lastMove.From] = Ref.ri.[lastMove.To] + 2) && not lastMovepc.IsW) 
                || ((Ref.ri.[lastMove.From] = Ref.ri.[lastMove.To] - 2) && lastMovepc.IsW))
            if lastMovepc.Img % 6 = 0 
                && Ref.fi.[lastMove.From] = Ref.fi.[lastMove.To] 
                && firstPmove then 
                " " + Ref.f.[lastMove.From] + (if lastMovepc.IsW then "3 "
                                                else "6 ") // The case between From and To
            else " - "
        else " - " // There is not en passant target square
    let FenPart5 p =
        String.Format("{0} ", 
                        (if p.Mhst.Length > 0 then p.Mhst.[0].FiftyCntr
                        else 0))
    let FenPart6 p = ((p.Trn >>> 1) + 1).ToString()
    let ToFen p = 
        // Field 1: Piece placement data
        let fen1 = FenPart1 p 
        // Field 2: Active colour
        let fen2 = FenPart2 p
        // Field 3: Castling availability
        let fen3 = FenPart3 p
        // Field 4: En passant target square coordonates
        let fen4 = FenPart4 p
        // Field 5: number of Halfmove clock or ply since the last pawn advance or capturing move.
        let fen5 = FenPart5 p
        // Field 6: Full move number
        let fen6 = FenPart6 p
        fen1 + fen2 + fen3 + fen4 + fen5 + fen6
    
    let Mpgn p = p.Mhst |> List.map (fun m -> m.PGN)
    
    let CreateMove(p, mvname, from, mto, sqcaptured, score) = 
        let mvpc = (p.Pcs |> List.filter (fun pc -> pc.Sq = from)).[0]
        
        let mFiftyMoveDrawCounter = 
            if (mvname <> NullMove && sqcaptured = -1 && mvpc.Img % 6 <> 0) then 
                (if p.Mhst.Length > 0 then 
                     p.Mhst.[p.Mhst.Length - 1].FiftyCntr + 1
                 else 1)
            else 0
        Mov.create(mvpc.Img, from, mto, sqcaptured, mvname, mFiftyMoveDrawCounter, "", "")
    
    //move based results
    let rec CanBmovs img ml psn = 
        match ml with
        | [] -> false
        | ml -> 
            let cord = ml.Head
            let pcl = psn.Pcs |> List.filter (fun pc -> pc.Sq = cord)
            if pcl.Length = 0 then CanBmovs img ml.Tail psn
            elif pcl.[0].IsW = psn.IsWhite && pcl.[0].Img % 6 = img then 
                true
            else CanBmovs img ml.Tail psn
    //get rays based results
    let rec CanBrays img r rl psn = 
        match r with
        | [] -> 
            if List.isEmpty rl then false
            else CanBrays img rl.Head rl.Tail psn
        | r -> 
            let cord = r.Head
            let pcl = psn.Pcs |> List.filter (fun pc -> pc.Sq = cord)
            if pcl.Length = 0 then CanBrays img r.Tail rl psn
            elif pcl.[0].IsW = psn.IsWhite 
                    && (pcl.[0].Img % 6 = img || pcl.[0].Img % 6 = 5) then true
            else if List.isEmpty rl then false
            else CanBrays img rl.Head rl.Tail psn
    let CanBeMovedToBy(ord, dif, psn, testK) = 
        let psn = 
            { psn with IsWhite = 
                           if dif then not psn.IsWhite
                           else psn.IsWhite }
        // Pawn
        let canbMv2P() = 
            CanBmovs 0 (if psn.IsWhite then Ref.attsPWto.[ord]
                        else Ref.attsPBto.[ord]) psn
        // Knight
        let canbMv2N() = CanBmovs 2 Ref.movsN.[ord] psn
        // Bishop & Queen
        let canbMv2B() = CanBrays 1 Ref.raysB.[ord].Head Ref.raysB.[ord].Tail psn
        // Rook & Queen
        let canbMv2R() = CanBrays 3 Ref.raysR.[ord].Head Ref.raysR.[ord].Tail psn
        // King!
        let canbMv2K() = CanBmovs 4 Ref.movsK.[ord] psn
        canbMv2P() || canbMv2N() || canbMv2B() || canbMv2R() 
        || (testK && canbMv2K())
    
    let IsInCheck (psn, testK) = 
        let kingsq = 
            if psn.IsWhite then 
                (psn.Pcs |> List.filter (fun pc -> pc.Img = 4)).[0].Sq
            else (psn.Pcs |> List.filter (fun pc -> pc.Img = 10)).[0].Sq
        CanBeMovedToBy(kingsq, true, psn, testK)
    
    let DoCastle mv (pc:Piece) =
        let sqR = 
            if mv.MvName = CastleK && pc.IsW then Ref.GetOrdFR(7, 0)
            elif mv.MvName = CastleK then Ref.GetOrdFR(7, 7)
            elif mv.MvName = CastleQ && pc.IsW then Ref.GetOrdFR(0, 0)
            elif mv.MvName = CastleQ then Ref.GetOrdFR(0, 7)
            else -1
        let nsqR = 
            if mv.MvName = CastleK then Ref.GetOrdFR(5, Ref.ri.[mv.To])
            elif mv.MvName = CastleQ then Ref.GetOrdFR(3, Ref.ri.[mv.To])
            else -1
        sqR,nsqR
    let DoProm mv psn pc =
            match mv.MvName with
            | PromB -> 
                { pc with Sq = mv.To
                          LastMv = psn.Trn
                          NoMvs = pc.NoMvs + 1
                          Img = 
                              if pc.IsW then 1
                              else 7 }
            | PromN -> 
                { pc with Sq = mv.To
                          LastMv = psn.Trn
                          NoMvs = pc.NoMvs + 1
                          Img = 
                              if pc.IsW then 2
                              else 8 }
            | PromR -> 
                { pc with Sq = mv.To
                          LastMv = psn.Trn
                          NoMvs = pc.NoMvs + 1
                          Img = 
                              if pc.IsW then 3
                              else 9 }
            | PromQ -> 
                { pc with Sq = mv.To
                          LastMv = psn.Trn
                          NoMvs = pc.NoMvs + 1
                          Img = 
                              if pc.IsW then 5
                              else 11 }
            | _ -> 
                { pc with Sq = mv.To
                          LastMv = psn.Trn
                          NoMvs = pc.NoMvs + 1 }
    let DoMoveOK(mv, psn) = 
        //do moves first
        let pc = (psn.Pcs |> List.filter (fun pc -> pc.Sq = mv.From)).[0]
        //do castling
        let sqR,nsqR = DoCastle mv pc
        //do promotion
        let npc = DoProm mv psn pc
        //do psn changes
        let updpc pci = 
            if pci.Sq = mv.From then npc
            elif sqR <> -1 && pci.Sq = sqR then { pci with Sq = nsqR }
            else pci
        
        let npsn = 
            if mv.SqCap <> -1 then 
                { psn with Pcs = 
                               psn.Pcs
                               |> List.filter (fun pc -> pc.Sq <> mv.SqCap)
                               |> List.map updpc }
            else { psn with Pcs = psn.Pcs |> List.map updpc }
        
        //check for invalidmove
        let isInCheck = IsInCheck (npsn, true)
        //Undo invalid moves
        if isInCheck then false, psn
        else 
            true, 
            { npsn with Trn = npsn.Trn + 1
                        Mhst = mv :: npsn.Mhst
                        IsWhite = not npsn.IsWhite }
    
    let Kmoved isw ks (psn:Posn) = 
        if isw then 
            if ks then not psn.CanK
            else not psn.CanQ
        else if ks then not psn.Cank
        else not psn.Canq
    let Kempty isw ks (psn:Posn) pcsqord = 
        if ks then 
            (psn.Pcs |> List.filter (fun pc -> pc.Sq = pcsqord + 1)).Length > 0 
            || (psn.Pcs |> List.filter (fun pc -> pc.Sq = pcsqord + 2)).Length > 0
        else 
            (psn.Pcs |> List.filter (fun pc -> pc.Sq = pcsqord - 1)).Length > 0 
            || (psn.Pcs |> List.filter (fun pc -> pc.Sq = pcsqord - 2)).Length > 0 
            || (psn.Pcs |> List.filter (fun pc -> pc.Sq = pcsqord - 3)).Length > 0
    let Kattack ks (psn:Posn) pcsqord = 
        //need to send other as true indicating testing wrong player to move to try
        let oth = true
        if ks then 
            CanBeMovedToBy(pcsqord + 1, oth, psn, true) 
            || CanBeMovedToBy(pcsqord + 2, oth, psn, true)
        else 
            CanBeMovedToBy(pcsqord - 1, oth, psn, true) 
            || CanBeMovedToBy(pcsqord - 2, oth, psn, true)

    let CanCastle(isw, ks, psn) = 
        let pc = 
            (psn.Pcs |> List.filter (fun pc -> 
                            if isw then pc.Img = 4
                            else pc.Img = 10)).[0]
        
        let pcsqord = pc.Sq
        // King hasnt moved
        let testkm() = Kmoved isw ks psn
        // All squares between King and Rook are unoccupied
        let testemp() = Kempty isw ks psn pcsqord
        // The king does not move over a square that is attacked by an enemy piece during the castling move
        let testattck() = Kattack ks psn pcsqord
        // King is not in check or above
        not (testkm() || testemp() || testattck() || IsInCheck (psn, false))
    
    let CanCastleK(psn) = CanCastle(true, true, psn)
    let CanCastleQ(psn) = CanCastle(true, false, psn)
    let CanCastlek(psn) = CanCastle(false, true, psn)
    let CanCastleq(psn) = CanCastle(false, false, psn)
    
    //ray based moves
    let rec findMovesRays r rl ml psn pc = 
        match r with
        | [] -> 
            if rl <> [] then findMovesRays rl.Head rl.Tail ml psn pc
            else ml
        | r -> 
            let cord = r.Head
            let pcl = psn.Pcs |> List.filter (fun pc -> pc.Sq = cord)
            if pcl.Length = 0 then 
                let oml = 
                    (CreateMove(psn, Standard, pc.Sq, cord, -1, 0)) :: ml
                findMovesRays r.Tail rl oml psn pc
            elif (pcl.[0].IsW <> pc.IsW && pcl.[0].Img % 6 <> 4) then 
                let oml = 
                    (CreateMove(psn, Standard, pc.Sq, cord, pcl.[0].Sq, 0)) 
                    :: ml
                if rl <> [] then findMovesRays rl.Head rl.Tail oml psn pc
                else oml
            else if rl <> [] then findMovesRays rl.Head rl.Tail ml psn pc
            else ml
    //do functions for OK moves, captures, non-captures
    let moveOK psn pc ord = 
        let pcl = psn.Pcs |> List.filter (fun pc -> pc.Sq = ord)
        if pcl.Length = 0 then 
            [ (CreateMove(psn, Standard, pc.Sq, ord, -1, 0)) ]
        elif (pcl.[0].IsW <> pc.IsW && pcl.[0].Img % 6 <> 4) then 
            [ (CreateMove(psn, Standard, pc.Sq, ord, pcl.[0].Sq, 0)) ]
        else []
    let capOKt psn (pc:Piece) ord typ = 
        let pcl = psn.Pcs |> List.filter (fun pc -> pc.Sq = ord)
        if pcl.Length > 0 && pcl.[0].IsW <> pc.IsW && pcl.[0].Img % 6 <> 4 then 
            [ (CreateMove(psn, typ, pc.Sq, ord, pcl.[0].Sq, 0)) ]
        else []
    let noncapOKt psn pc ord typ = 
        let pcl = psn.Pcs |> List.filter (fun p -> p.Sq = ord)
        if pcl.Length = 0 then 
            [ (CreateMove(psn, typ, pc.Sq, ord, -1, 0)) ]
        else []
    let capOK psn pc ord = capOKt psn pc ord Standard
    let noncapOK psn pc ord = noncapOKt psn pc ord Standard
    let GetPproms psn (pc:Piece) =
        let promotionTypes = [ PromQ; PromR; PromN; PromB ]
                
        // Captures
        let capords = 
            if pc.IsW then Ref.attsPW.[pc.Sq]
            else Ref.attsPB.[pc.Sq]
                
        let caps = 
            capords
            |> List.map 
                    (fun ord -> 
                    List.map (capOKt psn pc ord) promotionTypes |> List.concat)
            |> List.concat
                
        // Forward one
        let ord = 
            if pc.IsW then Ref.movPW.[pc.Sq]
            else Ref.movPB.[pc.Sq]
                
        let mvs = List.map (noncapOKt psn pc ord) promotionTypes |> List.concat
        caps @ mvs
    let GetPnmvs psn (pc:Piece) =
        // Captures
        let caps = 
            List.map (capOK psn pc) (if pc.IsW then Ref.attsPW.[pc.Sq] else Ref.attsPB.[pc.Sq])
            |> List.concat
                
        // Forward one
        let ord = 
            if pc.IsW then Ref.movPW.[pc.Sq]
            else Ref.movPB.[pc.Sq]
                
        let f1s = noncapOK psn pc ord
                
        // Forward two
        let ord2 = 
            if pc.IsW then Ref.movPW2.[pc.Sq]
            else Ref.movPB2.[pc.Sq]
                
        let f2s = 
            if ord2 <> -1 then 
                // Check one square ahead is not occupied
                let pcl = psn.Pcs |> List.filter (fun pc -> pc.Sq = ord)
                if pcl.Length = 0 then noncapOK psn pc ord2
                else []
            else []
        caps @ f1s @ f2s
    let GetPepl psn (pc:Piece) =
        // En Passent Left
        let epl = 
            if (Ref.epPWl.[pc.Sq] && pc.IsW 
                || Ref.epPBl.[pc.Sq] && not pc.IsW) then 
                let pcl = 
                    psn.Pcs 
                    |> List.filter 
                            (fun p -> p.Sq = Ref.epPlpc.[pc.Sq])
                if (pcl.Length > 0 && pcl.[0].NoMvs = 1 
                    && pcl.[0].LastMv = psn.Trn - 1 
                    && pcl.[0].Img % 6 = 0 && pcl.[0].IsW <> pc.IsW) then 
                    let ord = 
                        if pc.IsW then Ref.epPWlto.[pc.Sq]
                        else Ref.epPBlto.[pc.Sq]
                    [ (CreateMove(psn, EnPassent, pc.Sq, ord, pcl.[0].Sq, 0)) ]
                else []
            else []
        epl
    let GetPepr psn (pc:Piece) =
        // En Passent Right
        let epr = 
            if (Ref.epPWr.[pc.Sq] && pc.IsW 
                || Ref.epPBr.[pc.Sq] && not pc.IsW) then 
                let pcl = 
                    psn.Pcs 
                    |> List.filter 
                            (fun p -> p.Sq = Ref.epPrpc.[pc.Sq])
                if (pcl.Length > 0 && pcl.[0].NoMvs = 1 
                    && pcl.[0].LastMv = psn.Trn - 1 
                    && pcl.[0].Img % 6 = 0 && pcl.[0].IsW <> pc.IsW) then 
                    let ord = 
                        if pc.IsW then Ref.epPWrto.[pc.Sq]
                        else Ref.epPBrto.[pc.Sq]
                    [ (CreateMove(psn, EnPassent, pc.Sq, ord, pcl.[0].Sq, 0)) ]
                else []
            else []
        epr
    let GetPmvs psn (pc:Piece) =
        let blnIsPromotion = 
            if pc.IsW then Ref.promPW.[pc.Sq]
            else Ref.promPB.[pc.Sq]
        if blnIsPromotion then GetPproms psn pc
        else 
            let nmvs = GetPnmvs psn pc
            let epl = GetPepl psn pc
            let epr = GetPepr psn pc
            nmvs @ epl @ epr

    let GenLazyMoves pc psn = 
        let pcsqord = pc.Sq
        let pccol = pc.IsW
        match pc.Img with
        | 0 | 6 -> GetPmvs psn pc
        | 1 | 7 -> 
            findMovesRays Ref.raysB.[pcsqord].Head Ref.raysB.[pcsqord].Tail [] psn pc
        | 2 | 8 -> 
            let ords = Ref.movsN.[pcsqord]
            List.map (moveOK psn pc) ords |> List.concat
        | 3 | 9 -> 
            findMovesRays Ref.raysR.[pcsqord].Head Ref.raysR.[pcsqord].Tail [] psn pc
        | 5 | 11 -> 
            findMovesRays Ref.raysQ.[pcsqord].Head Ref.raysQ.[pcsqord].Tail [] psn pc
        | 4 | 10 -> 
            let ords = Ref.movsK.[pcsqord]
            let mvs = List.map (moveOK psn pc) ords |> List.concat
            
            let cK = 
                if (pccol && CanCastleK(psn)) then 
                    [ (CreateMove(psn, CastleK, pcsqord, pcsqord + 2, -1, 0)) ]
                else []
            
            let ck = 
                if (not pccol && CanCastlek(psn)) then 
                    [ (CreateMove(psn, CastleK, pcsqord, pcsqord + 2, -1, 0)) ]
                else []
            
            let cQ = 
                if (pccol && CanCastleQ(psn)) then 
                    [ (CreateMove(psn, CastleQ, pcsqord, pcsqord - 2, -1, 0)) ]
                else []
            
            let cq = 
                if (not pccol && CanCastleq(psn)) then 
                    [ (CreateMove(psn, CastleQ, pcsqord, pcsqord - 2, -1, 0)) ]
                else []
            
            mvs @ cK @ ck @ cQ @ cq
        | _ -> failwith "invalid image for piece"
    
    let GenLegalMvs(pc, psn) = 
        (GenLazyMoves pc psn) |> List.filter (fun m -> fst (DoMoveOK(m, psn)))
    
    /// generates all legal moves from a position
    let GenLegalMvsPr psn = 
        psn.Pcs
        |> List.map (fun pc -> 
               if pc.IsW = psn.IsWhite then GenLegalMvs(pc, psn)
               else [])
        |> List.concat
    
    let IsInCheckMate psn = 
        //this is only called if in check is known
        //check for any pc
        let checkpc pc = 
            let mvl = GenLazyMoves pc psn
            
            let rec validmv ml = 
                if List.isEmpty ml then false
                else 
                    let move = ml.Head
                    if fst (DoMoveOK(move, psn)) then true
                    else validmv ml.Tail
            validmv mvl
        
        let rec nolegalexists pcl = 
            if List.isEmpty pcl then true
            else 
                let p : Piece = pcl.Head
                if p.IsW = psn.IsWhite then 
                    if checkpc p then false
                    else nolegalexists pcl.Tail
                else nolegalexists pcl.Tail
        
        nolegalexists psn.Pcs
    
    let DoMove(mvi, ipsn) = 
        let strfrom = Ref.sq.[mvi.From]
        let strto = Ref.sq.[mvi.To]
        let moves = GenLegalMvsPr ipsn
        let dupfrom = ref ""
        
        let rec testdup mvl = 
            match mvl with
            | [] -> ()
            | _ -> 
                let move = mvl.Head
                if Ref.sq.[move.To] = strto 
                   && Piece.letr (mvi.MvPcImg) = Piece.letr (move.MvPcImg) 
                   && Ref.sq.[move.From] <> strfrom then 
                    dupfrom := Ref.sq.[move.From]
                else testdup mvl.Tail
        testdup moves
        let _, psn = DoMoveOK(mvi, ipsn)
        let incheck = IsInCheck (psn, false)
        
        let incheckmate = 
            if incheck then IsInCheckMate(psn)
            else false
        
        //update mv in history
        let mv = psn.Mhst.Head
        let pgn = Mov.PGN(mv, !dupfrom, incheckmate, incheck)
        let uci = Mov.uci (mv)
        { psn with Mhst = 
                       { mv with PGN = pgn
                                 UCI = uci } :: psn.Mhst.Tail }
    
    let rec DoMoves(mvl, ipsn) = 
        if List.isEmpty mvl then ipsn
        else 
            let opsn = DoMove(mvl.Head, ipsn)
            DoMoves(mvl.Tail, opsn)
    
    /// Do a move on a position given a from and a to
    let DoMoveft((mfrom, mto, mn), psn) = 
        let moves = GenLegalMvsPr psn
        
        let rec domv (mvl : Move list) = 
            if List.isEmpty mvl then psn
            else 
                let mv = mvl.Head
                if (mn = OMoveName.NullMove|| mv.MvName = mn) && mv.From = mfrom && mv.To = mto then 
                    DoMove(mv, psn)
                else domv mvl.Tail
        domv moves
    
    /// Do a list of moves on a position given a from and a to
    let rec DoMovesft((mvlft : (int * int * OMoveName) list), ipsn) = 
        if List.isEmpty mvlft then ipsn
        else 
            let opsn = DoMoveft(mvlft.Head, ipsn)
            DoMovesft(mvlft.Tail, opsn)
    
    /// Find a move given UCI move and a position
    let FndMv((ucimv : string), psn) = 
        let moves = GenLegalMvsPr psn
        let f, t, mn = Mov.fromuci ucimv
        
        let rec fndmv (mvl : Move list) = 
            if List.isEmpty mvl then None
            else 
                let mv = mvl.Head
                if (mn = OMoveName.NullMove || mv.MvName = mn) && mv.From = f && mv.To = t then 
                    Some(mv)
                else fndmv mvl.Tail
        fndmv moves
    
    let FndMvPGN((pgnmv : string), psn) = 
        let moves = GenLegalMvsPr psn
        
        let getpgn mv = 
            let npsn = DoMove(mv, psn)
            npsn.Mhst.Head
        
        let mvl = moves |> List.map getpgn
        
        let rec fndmv (mvl : Move list) = 
            if List.isEmpty mvl then None
            else 
                let mv = mvl.Head
                if pgnmv = mv.PGN then Some(mv)
                else fndmv mvl.Tail
        fndmv mvl
    
    /// mvl2str - generates array of string of moves from movlist
    let rec mvl2str (mvl : Move list) str = 
        if List.isEmpty mvl then str
        else mvl2str mvl.Tail (str + " " + (mvl.Head.UCI))
    
    /// psn2str - generates array of string of moves from position
    let psn2str (psn : Posn) = (mvl2str (List.rev psn.Mhst) "").Trim()
    
    /// mvl2pgn - generates array of pgnmoves from movlist
    let rec mvl2pgn (mvl : Move list) str = 
        if List.isEmpty mvl then str
        else mvl2pgn mvl.Tail (str + " " + (mvl.Head.PGN))
    
    /// psn2pgn - generates array of pgn moves from position
    let psn2pgn (psn : Posn) = (mvl2pgn (List.rev psn.Mhst) "").Trim()
    
    /// psn2pgnf - generates array of pgn file string from position
    let psn2pgnf (psn : Posn) = 
        let nl = Environment.NewLine
        let hdr = 
            "[Event \"?\"]" + nl + "[Site \"?\"]" + nl + "[Date \"????.??.??\"]" 
            + nl + "[Round \"?\"]" + nl + "[White \"?\"]" + nl + "[Black \"?\"]" 
            + nl + "[Result \"*\"]"
        hdr + nl + (psn |> psn2pgn) + " *"
    
    ///get pgn games given name
    let loadPGN nm = 
        let opts = Opts.load()
        let pgnfil = Path.Combine(opts.Gmfol, nm)
        let gms = Lizard.PGN.ReadFromFile pgnfil
        gms

    let PgnTo fmvs sqto =
        let fmvs1 = fmvs |> List.filter (fun m -> m.To = sqto)
        if fmvs1.Length = 0 then failwith "No moves found"
        else fmvs1
    let PgnFrom fmvs pgnmv =
        let fmvs1 = 
            if pgnmv.Mfrom <> -1 then 
                fmvs |> List.filter (fun m -> m.From = pgnmv.Mfrom)
            else fmvs
        if fmvs1.Length = 0 then failwith "No moves found"
        else fmvs1
    let PgnfFrom fmvs pgnmv =
        let fmvs1 = 
            if pgnmv.Mffrom <> -1 then 
                fmvs 
                |> List.filter 
                        (fun m -> Ref.fi.[m.From] = pgnmv.Mffrom)
            else fmvs
        if fmvs1.Length = 0 then failwith "No moves found"
        else fmvs1
    let PgnrFrom fmvs pgnmv =
        let fmvs1 = 
            if pgnmv.Mrfrom <> -1 then 
                fmvs 
                |> List.filter 
                        (fun m -> 
                        Ref.ri.[m.From] = pgnmv.Mrfrom)
            else fmvs
        if fmvs1.Length = 0 then failwith "No moves found"
        else fmvs1
    let PgnType fmvs pgnmv =
        let fmvs1 = 
            if pgnmv.Mtyp <> NullMove then 
                fmvs 
                |> List.filter 
                        (fun m -> m.MvName = pgnmv.Mtyp)
            else fmvs
        if fmvs1.Length = 0 then 
            failwith "No moves found"
        else fmvs1
    let PgnValid fmvs pos sqto pc =
        let fmvs1 = 
            fmvs 
            |> List.filter 
                    (fun m -> fst (DoMoveOK(m, pos)))
        if fmvs1.Length = 0 then 
            failwith "No moves found"
        elif fmvs1.Length = 1 then fmvs1.[0]
        else 
            failwith 
                ("Too many moves to " + Ref.f.[sqto] 
                    + Ref.r.[sqto] + " for " 
                    + (Piece.letr pc))
    let Pgn2Main pos pgnmv =
            let pc = 
                if pos.IsWhite then pgnmv.MWimg
                else pgnmv.MWimg + 6
            
            let fil1 = pos.Pcs |> List.filter (fun p -> p.Img = pc)
            let sqto = pgnmv.Mto
            
            let fil2 = 
                if sqto <> -1 && pgnmv.MWimg = 0 then 
                    fil1 
                    |> List.filter 
                           (fun p -> abs (Ref.fi.[p.Sq] - Ref.fi.[sqto]) < 2)
                else fil1
            
            let mvs = 
                fil2
                |> List.map (fun p -> GenLazyMoves p pos)
                |> List.concat
            
            if mvs.Length = 0 then failwith "No moves found"
            elif mvs.Length = 1 then mvs.[0]
            else 
                let fmvs = PgnTo mvs sqto
                if fmvs.Length = 1 then fmvs.[0]
                else 
                    //need to apply to, file or rank clarifier
                    let fmvs1 = PgnFrom fmvs pgnmv
                    if fmvs1.Length = 1 then fmvs1.[0]
                    else 
                        let fmvs2 = PgnfFrom fmvs1 pgnmv
                        if fmvs2.Length = 1 then fmvs2.[0]
                        else 
                            let fmvs3 = PgnrFrom fmvs2 pgnmv
                            if fmvs3.Length = 1 then fmvs3.[0]
                            else 
                                let fmvs4 = PgnType fmvs3 pgnmv
                                if fmvs4.Length = 1 then fmvs4.[0]
                                else PgnValid fmvs3 pos sqto pc

    ///get mv given pgn mv and pos
    let pgn2mov pos pgnmv = 
        let mvtyp = pgnmv.Mtyp
        match mvtyp with
        | CastleK -> 
            let pcsqord = 
                if pos.IsWhite then 4
                else 60
            CreateMove(pos, CastleK, pcsqord, pcsqord + 2, -1, 0)
        | CastleQ -> 
            let pcsqord = 
                if pos.IsWhite then 4
                else 60
            CreateMove(pos, CastleQ, pcsqord, pcsqord - 2, -1, 0)
        | _ -> 
            Pgn2Main pos pgnmv
    
    ///get pos given pgn
    let pgn2pos pgn = 
        let rec getpos pmvl pos = 
            if List.isEmpty pmvl then pos
            else 
                let mt = pmvl.Head
                match mt with
                | MovePair(mp) -> 
                    let wm = mp.MvW
                    let mv1 = pgn2mov pos mp.MvW
                    let pos1 = DoMove(mv1, pos)
                    let mv2 = pgn2mov pos1 mp.MvB
                    let pos2 = DoMove(mv2, pos1)
                    getpos pmvl.Tail pos2
                | SingleMove(sm) -> 
                    let mv1 = pgn2mov pos sm.Mv
                    let pos1 = DoMove(mv1, pos)
                    getpos pmvl.Tail pos1
                | _ -> getpos pmvl.Tail pos
        
        let pmvs = pgn.MoveText
        let pos = getpos pmvs st
        pos