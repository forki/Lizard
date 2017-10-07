namespace Lizard

open System

module Ref = 
    let fs = [ 'a'..'h' ]
    let private rs = [ 1..8 ] |> List.rev
    let f = 
        let fileName (sq) = fs.[sq % 8].ToString()
        [ for i in 0..63 -> fileName (i) ]
    
    let r = 
        let rankName (sq) = rs.[sq / 8].ToString()
        [ for i in 0..63 -> rankName (i) ]
    
    let sq = List.map2 (+) f r
    
