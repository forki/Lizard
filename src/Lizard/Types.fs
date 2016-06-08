namespace Lizard

open System.Text
open System
open System.Windows.Forms

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
    
    //storage of variations
    type Varn = 
        { Name : string
          Isw : bool
          Brchs : Move list list }
    
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
          Emaxdepth : int
          Elog : bool
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
