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
    
    /// Index of square on the board
    type Sq = int
    
    /// Fast type for making moves on board
    type Move = 
        { Mfrom : Sq
          Mto : Sq
          Mtyp : MvTyp option
          Mpgn : string }
        override x.ToString() = x.Mpgn
    
