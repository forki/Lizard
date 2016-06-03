module Posn

open FsUnit
open NUnit.Framework
open Lizard

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
     let gms = PGN.ReadFromString weprPgn
     let gm = gms.Head
     gm.Moves.Head.Mpgn|>should equal "d4"
     let rmvs = gm.Moves|>List.rev
     let lmv = rmvs.Head
     lmv.Mpgn|> should equal "dxe6"

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
     let gms = PGN.ReadFromString weplPgn
     let gm = gms.Head
     gm.Moves.Head.Mpgn|>should equal "d4"
     let rmvs = gm.Moves|>List.rev
     let lmv = rmvs.Head
     lmv.Mpgn|> should equal "dxc6"

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
     let gms = PGN.ReadFromString beprPgn
     let gm = gms.Head
     gm.Moves.Head.Mpgn|>should equal "c3"
     let rmvs = gm.Moves|>List.rev
     let lmv = rmvs.Head
     lmv.Mpgn|> should equal "exf3"

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
     let gms = PGN.ReadFromString beplPgn
     let gm = gms.Head
     gm.Moves.Head.Mpgn|>should equal "c3"
     let rmvs = gm.Moves|>List.rev
     let lmv = rmvs.Head
     lmv.Mpgn|> should equal "exd3"
