namespace Lizard

open System

[<AutoOpen>]
module Types = 
    /// Move type where not a simple move
    type MvTyp = 
        | Prom of char
        | CasK
        | CasQ
        | Ep
        | Standard
        | Invalid
    /// Move eval
    type MvEval = 
        | Normal
        | Excellent
        | Weak
        | Surprising
    
    /// Index of square on the board
    type Sq = int
    
    /// Fast type for making moves on board
    type Move = 
        { Mfrom : Sq
          Mto : Sq
          Mtyp : MvTyp
          Mpgn : string }
        override x.ToString() = x.Mpgn
        member x.UCI = 
            let mv = Ref.sq.[x.Mfrom] + Ref.sq.[x.Mto]
            match x.Mtyp with
            | Prom(t) -> mv + t.ToString()
            | _ -> mv
    /// Move type including eval
    type Move1 = 
        { Mfrom : Sq
          Mto : Sq
          Mtyp : MvTyp
          Mpgn : string
          Meval : MvEval
          Scr10 : int
          Scr25 : int 
          Bresp : string
          ECO : string
          FicsPc : float}
        override x.ToString() = x.Mpgn
        member x.UCI = 
            let mv = Ref.sq.[x.Mfrom] + Ref.sq.[x.Mto]
            match x.Mtyp with
            | Prom(t) -> mv + t.ToString()
            | _ -> mv
    
    //storage of variations
    type Line = 
        { Mvs : Move list }
    type Varn1 = 
        { Name : string
          Isw : bool
          Brchs : Line list }
    type Line1 = 
        { Mvs : Move1 list }
    type Varn = 
        { Name : string
          Isw : bool
          Brchs : Line1 list }
    
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
          Rnum : int
          Rskip : int
          Lnum : int
          Lskip : int
          Emaxdepth : int
          Elog : bool }
    
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
