namespace LizardChess

open System
open System.Linq

module Piece = 
    let create (sq, lastmv, nomvs, img) = 
        { Sq = sq
          LastMv = lastmv
          NoMvs = nomvs
          Img = img }
    
    let letr img = 
        match img with
        | 0 -> "P"
        | 1 -> "B"
        | 2 -> "N"
        | 3 -> "R"
        | 4 -> "K"
        | 5 -> "Q"
        | 6 -> "p"
        | 7 -> "b"
        | 8 -> "n"
        | 9 -> "r"
        | 10 -> "k"
        | 11 -> "q"
        | _ -> ""
    
    let chr img = 
        match img with
        | 0 -> 'P'
        | 1 -> 'B'
        | 2 -> 'N'
        | 3 -> 'R'
        | 4 -> 'K'
        | 5 -> 'Q'
        | 6 -> 'p'
        | 7 -> 'b'
        | 8 -> 'n'
        | 9 -> 'r'
        | 10 -> 'k'
        | 11 -> 'q'
        | _ -> ' '