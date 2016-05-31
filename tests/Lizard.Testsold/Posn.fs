module Posn

open FsUnit
open NUnit.Framework
open LizardChess.Posn

let weprPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     d4 d6 d5 e5 dxe6 *"

[<Test>]
let ``Posn pgn2pos wepr``() =
     let gms = LizardChess.PgnParser.ReadFromString weprPgn
     let pos = pgn2pos gms.Head
     let lmv = pos.Mhst.Head
     lmv.PGN|> should equal "dxe6"

let weplPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     d4 d6 d5 c5 dxc6 *"

[<Test>]
let ``Posn pgn2pos wepl``() =
     let gms = LizardChess.PgnParser.ReadFromString weplPgn
     let pos = pgn2pos gms.Head
     let lmv = pos.Mhst.Head
     lmv.PGN|> should equal "dxc6"

let beprPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     c3 e5 b3 e4 f4 exf3 *"

[<Test>]
let ``Posn pgn2pos bepr``() =
     let gms = LizardChess.PgnParser.ReadFromString beprPgn
     let pos = pgn2pos gms.Head
     let lmv = pos.Mhst.Head
     lmv.PGN|> should equal "exf3"

let beplPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     c3 e5 b3 e4 d4 exd3 *"

[<Test>]
let ``Posn pgn2pos bepl``() =
     let gms = LizardChess.PgnParser.ReadFromString beplPgn
     let pos = pgn2pos gms.Head
     let lmv = pos.Mhst.Head
     lmv.PGN|> should equal "exd3"
