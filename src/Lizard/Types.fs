namespace LizardChess

open System
open System.Threading
open System.Linq

[<AutoOpen>]
module Types = 
    //types of moves
    type OMoveName = 
        | Standard
        | CastleQ
        | CastleK
        | PromQ
        | PromR
        | PromN
        | PromB
        | EnPassent
        | NullMove
        member x.Vl = 
            match x with
            | PromQ -> 5
            | PromR -> 3
            | PromN -> 2
            | PromB -> 1
            | _ -> 0
    
    //piece
    type Piece = 
        { Sq : int
          LastMv : int
          NoMvs : int
          Img : int }
        member x.IsW = x.Img < 6
    
    //move details
    type Move = 
        { MvPcImg : int
          From : int
          To : int
          SqCap : int
          MvName : OMoveName
          FiftyCntr : int
          PGN : string
          UCI : string }
        member x.IsW = x.MvPcImg < 6
    
    //posn - used for current position but also as analysis progresses
    type Posn = 
        { Pcs : Piece list
          Trn : int
          Mhst : Move list
          IsWhite : bool }
        
        member p.Sqrs = 
            let sqs = Array.create 64 -1
            p.Pcs |> List.iter (fun pc -> sqs.[pc.Sq] <- pc.Img)
            sqs
        
        member p.CanK = 
            let wkl = p.Pcs |> List.filter (fun pc -> pc.Sq = 4)
            let wknotmoved = wkl.Length = 1 && wkl.[0].Img = 4 && wkl.[0].NoMvs = 0
            let wkrl = p.Pcs |> List.filter (fun pc -> pc.Sq = 7)
            let wkrnotmoved = wkrl.Length = 1 && wkrl.[0].Img = 3 && wkrl.[0].NoMvs = 0
            wknotmoved && wkrnotmoved
        
        member p.CanQ = 
            let wkl = p.Pcs |> List.filter (fun pc -> pc.Sq = 4)
            let wknotmoved = wkl.Length = 1 && wkl.[0].Img = 4 && wkl.[0].NoMvs = 0
            let wqrl = p.Pcs |> List.filter (fun pc -> pc.Sq = 0)
            let wqrnotmoved = wqrl.Length = 1 && wqrl.[0].Img = 3 && wqrl.[0].NoMvs = 0
            wknotmoved && wqrnotmoved
        
        member p.Cank = 
            let bkl = p.Pcs |> List.filter (fun pc -> pc.Sq = 60)
            let bknotmoved = bkl.Length = 1 && bkl.[0].Img = 10 && bkl.[0].NoMvs = 0
            let bkrl = p.Pcs |> List.filter (fun pc -> pc.Sq = 63)
            let bkrnotmoved = bkrl.Length = 1 && bkrl.[0].Img = 9 && bkrl.[0].NoMvs = 0
            bknotmoved && bkrnotmoved
        
        member p.Canq = 
            let bkl = p.Pcs |> List.filter (fun pc -> pc.Sq = 60)
            let bknotmoved = bkl.Length = 1 && bkl.[0].Img = 10 && bkl.[0].NoMvs = 0
            let bqrl = p.Pcs |> List.filter (fun pc -> pc.Sq = 56)
            let bqrnotmoved = bqrl.Length = 1 && bqrl.[0].Img = 9 && bqrl.[0].NoMvs = 0
            bknotmoved && bqrnotmoved
    
    //storage of variations
    type Varn = 
        { Name : string
          Isw : bool
          Brchs : Posn list }
    
    //test - records of tests
    type TestDet = 
        { Mvl : Move list
          Mv : Move
          Vnname : string
          Status : string }
    
    //tstres - test result
    type Tstres = 
        { Vname : string
          Visw : bool
          Dte : DateTime
          Res : int }
    
    //linstr - store of analysis
    type Linstr = System.Collections.Generic.Dictionary<string, string>
    
    //enganl - record of engine analysis results
    type Enganl = 
        { Depth : int
          Scr : int
          Bestmv : string
          Resp : string
          BmPGN : string
          RmPGN : string }
    
    //engbm - record of engine best moves
    type Engbm = 
        { Bnum : int
          Bisw : bool
          BPGN : string
          Bstr : string
          Bscr : int }
    
    //options - record of all options
    type Options = 
        { Opnfol : string
          Tstfol : string
          Engfol : string
          Gmfol : string
          Rnum : int
          Rskip : int
          Lnum : int
          Lskip : int
          Eng : string
          Emindepth : int
          Emaxdepth : int
          Elog : bool
          Geng : string
          Gsecpm : int
          Guseopn : bool
          Funame : string
          Fpass : string
          Ftime : int
          Fuopn : bool }
    
    //ficsdata
    type Ficsmv = 
        { Fpgn : string
          Wpc : float
          Dpc : float
          Bpc : float
          Fnum : int }
    
    type Ficsdata = 
        { FENlong : string
          ECO : string
          ECOName : string
          NumGames : int
          MvList : Ficsmv [] }
    
    //fdbstr - store of Fics DB data
    type Fdbstr = System.Collections.Generic.Dictionary<string, Ficsdata>
