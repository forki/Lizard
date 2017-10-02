module Pos

open FsUnitTyped
open NUnit.Framework
open Lizard

[<Test>]
let ``Pos pawn mv``() =
    let pos = Pos.FromString "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w"
    let ans = pos.GetMvFT(51,35)
    ans.Mpgn|>shouldEqual "d4"

[<Test>]
let ``Pos pawn cap``() =
    let pos = Pos.FromString "rnbqkb1r/p2ppppp/5n2/1ppP4/2P5/8/PP2PPPP/RNBQKBNR w"
    let ans = pos.GetMvFT(34,25)
    ans.Mpgn|>shouldEqual "cxb5"

[<Test>]
let ``Pos pawn cap ep``() =
    let pos = Pos.FromString "r4rk1/1b1nq1pp/p3p3/1ppnPpN1/3P4/3Q1N2/PP2BPPP/R4RK1 w"
    let ans = pos.GetMvFT(28,21)
    ans.Mpgn|>shouldEqual "exf6"
    ans.Mtyp|>shouldEqual Ep

[<Test>]
let ``Pos bishop cap``() =
    let pos = Pos.FromString "rnbqkb1r/3ppp1p/P4np1/2pP4/8/2N5/PP2PPPP/R1BQKBNR b"
    let ans = pos.GetMvFT(2,16)
    ans.Mpgn|>shouldEqual "Bxa6"

[<Test>]
let ``Pos knight dup``() =
    let pos = Pos.FromString "rn1qk2r/4ppbp/b2p1np1/2pP4/8/2N2NP1/PP2PPBP/R1BQK2R b"
    let ans = pos.GetMvFT(1,11)
    ans.Mpgn|>shouldEqual "Nbd7"

[<Test>]
let ``Pos knight dup2``() =
    let pos = Pos.FromString "r1b1kbnr/pp3ppp/2n1p3/q1ppP3/3P2Q1/8/PPPN1PPP/R1B1KBNR w"
    let ans = pos.GetMvFT(62,45)
    ans.Mpgn|>shouldEqual "Ngf3"

[<Test>]
let ``Pos knight dup3``() =
    let pos = Pos.FromString "r2qrnk1/1bp2ppp/1p1p1n2/p2Np3/2PPP3/3B1NP1/PP1Q1P1P/R3R1K1 b"
    let ans = pos.GetMvFT(21,11)
    ans.Mpgn|>shouldEqual "N6d7"

[<Test>]
let ``Pos knight dup cap``() =
    let pos = Pos.FromString "rn2k2r/pp1Bqpbp/3p1np1/2pP4/4PB2/2N5/PP2NPPP/R2QK2R b"
    let ans = pos.GetMvFT(1,11)
    ans.Mpgn|>shouldEqual "Nbxd7"

[<Test>]
let ``Pos rook dup``() =
    let pos = Pos.FromString "r4rk1/3nppbp/b2p1np1/q1pP4/8/2N2NP1/PP1BPPBP/1R1Q1RK1 b"
    let ans = pos.GetMvFT(5,1)
    ans.Mpgn|>shouldEqual "Rfb8"

[<Test>]
let ``Pos castle``() =
    let pos = Pos.FromString "r2qk2r/3nppbp/b2p1np1/2pP4/8/2N2NP1/PP2PPBP/1RBQK2R b"
    let ans = pos.GetMvFT(4,6)
    ans.Mpgn|>shouldEqual "O-O"
