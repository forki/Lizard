namespace LizardChess

open System
open System.Linq
open System.IO

module Db = 
    ///convert move to int
    let mv2num (mv : Move) = mv.From + (mv.To <<< 6) + (mv.MvName.Vl <<< 12)
    
    ///convert in to from,to,prom pc
    let num2mv (num : int) = 
        let prm = num >>> 12
        let mto = (num - (prm <<< 12)) >>> 6
        let mfrom = num - (prm <<< 12) - (mto <<< 6)
        mfrom, mto, prm
    
    /// zeroise bit in uint64
    let ZeroBit position value = value &&& ~~~(1UL <<< position)
    
    /// set bit in uint64
    let SetBit position value = value ||| (1UL <<< position)
    
    /// get string for uint64
    let GetInt64BinStr(n : uint64) = 
        let b = Array.create 64 '0'
        for i = 0 to 63 do
            if ((n &&& (1UL <<< i)) <> 0UL) then b.[63 - i] <- '1'
        b |> System.String.Concat
    
    /// convert pos to array of uint64
    let pos2psqs pos = 
        let pcs = pos.Pcs
        let psqs = Array.create 12 0UL
        let setbit pc = psqs.[pc.Img] <- SetBit pc.Sq psqs.[pc.Img]
        pcs |> List.iter setbit
        psqs
    
    /// convert array of uint64 to str of board
    let psqs2str (psqs : uint64 []) = 
        let b = Array.create 64 '-'
        
        let setch j psq = 
            for i = 0 to 63 do
                if ((psq &&& (1UL <<< i)) <> 0UL) then b.[i] <- Piece.chr j
        psqs |> Array.iteri setch
        let str = b |> System.String.Concat
        let nl = System.Environment.NewLine
        str.[56..63] + nl + str.[48..55] + nl + str.[40..47] + nl + str.[32..39] 
        + nl + str.[24..31] + nl + str.[16..23] + nl + str.[8..15] + nl 
        + str.[0..7]