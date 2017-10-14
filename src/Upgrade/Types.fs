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
    
    ///// Move type including eval
    //type Move1 = 
    //    { Mfrom : Sq
    //      Mto : Sq
    //      Mtyp : MvTyp
    //      Mpgn : string
    //      Meval : MvEval
    //      Scr10 : int
    //      Scr25 : int 
    //      Bresp : string
    //      ECO : string
    //      FicsPc : float}
    //    override x.ToString() = x.Mpgn
    //    member x.UCI = 
    //        let mv = Ref.sq.[x.Mfrom] + Ref.sq.[x.Mto]
    //        match x.Mtyp with
    //        | Prom(t) -> mv + t.ToString()
    //        | _ -> mv
    
    ////storage of variations
    //type Line1 = 
    //    { Mvs : Move1 list }
    //type Varn = 
    //    { Name : string
    //      Isw : bool
    //      Brchs : Line1 list }
    
    /// Move type including eval
    type Move2 = 
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
    type Line2 = 
        { ECO : string
          Mvs : Move2 list }
    type Varn2 = 
        { Name : string
          Isw : bool
          ECO : string
          Lines : Line2 list }

    /// Move type including eval
    type Move = 
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
        { ECO : string
          Mvs : Move list }
    type Varn = 
        { Name : string
          Isw : bool
          ECO : string
          Lines : Line list }

