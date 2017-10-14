module Varn

open FsUnitTyped
open NUnit.Framework
open Lizard
open System.IO

let weprPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     d4 d6 d5 e5 dxe6 *"
let weplPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     d4 d6 d5 c5 dxc6 *"
let beprPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     c3 e5 b3 e4 f4 exf3 *"
let beplPgn = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     c3 e5 b3 e4 d4 exd3 *"
let beplPgn2 = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     c4 e5 b3 e4 d4 exd3 *"
let beplPgn3 = 
    "[Event \"?\"]
     [Site \"?\"]
     [Date \"????.??.??\"]
     [Round \"?\"]
     [White \"?\"]
     [Black \"?\"]
     [Result \"*\"]
     c3 e5 b3 e4 *"

let getmvl pgn =
     let gms = PGN.ReadFromString pgn
     let gm = gms.Head
     {ECO="";Mvs=gm.Moves}

let mvl1 = getmvl weprPgn
let mvl2 = getmvl weplPgn
let mvl3 = getmvl beprPgn
let mvl4 = getmvl beplPgn
let mvll = [mvl1;mvl2;mvl3;mvl4]
let mvl5 = getmvl beplPgn2
let mvl6 = getmvl beplPgn3
let vrn = {Varn.emp with Lines=mvll}

[<Test>]
let ``Varn cur``() =
    let ans = Varn.cur ("Test", true)
    ans.Name|>shouldEqual "Test"
    ans.Isw|>shouldEqual true
    ans.Lines.Length|>shouldEqual 0

[<Test>]
let ``Varn emp``() =
    let ans = Varn.emp
    ans.Name|>shouldEqual "NotSet"
    ans.Isw|>shouldEqual true
    ans.Lines.Length|>shouldEqual 0

[<Test>]
let ``Varn findsv``() =
    let ans = Varn.findsv mvl3.Mvs mvll
    let ans2 = Varn.findsv [] mvll
    let ans3 = Varn.findsv mvl5.Mvs mvll
    ans.Value|>shouldEqual 2
    ans2.Value|>shouldEqual 0
    ans3|>shouldEqual None

[<Test>]
let ``Varn findnmvs``() =
    let ans = Varn.findnmvs mvl3.Mvs mvll
    let ans2 = Varn.findnmvs mvl6.Mvs mvll
    ans.Length|>shouldEqual 0
    ans2.Length|>shouldEqual 2
    ans2.[0].Mpgn|>shouldEqual "d4"
    ans2.[1].Mpgn|>shouldEqual "f4"

[<Test>]
let ``Varn smmv``() =
    let ans = Varn.smmv mvl3.Mvs mvl4.Mvs 0
    let ans2 = Varn.smmv mvl1.Mvs mvl2.Mvs 0
    let ans3 = Varn.smmv mvl1.Mvs mvl3.Mvs 0
    ans|>shouldEqual 4
    ans2|>shouldEqual 3
    ans3|>shouldEqual 0

[<Test>]
let ``Varn fndidx``() =
    let ans = Varn.fndidx mvll 0 0 0 mvl1.Mvs
    let ans2 = Varn.fndidx mvll 0 0 0 mvl2.Mvs
    let ans3 = Varn.fndidx mvll 0 0 0 mvl4.Mvs
    let ans4 = Varn.fndidx mvll 0 0 0 mvl5.Mvs
    let ans5 = Varn.fndidx mvll 0 0 0 mvl6.Mvs
    ans|>shouldEqual 0
    ans2|>shouldEqual 1
    ans3|>shouldEqual 3
    ans4|>shouldEqual 3
    ans5|>shouldEqual 3

[<Test>]
let ``Varn mrgbrch``() =
    let ans = Varn.mrgbrch mvl1.Mvs mvll
    let ans2 = Varn.mrgbrch mvl5.Mvs mvll
    let ans3 = Varn.mrgbrch mvl6.Mvs mvll
    ans.Length|>shouldEqual 4
    ans2.Length|>shouldEqual 5
    ans3.Length|>shouldEqual 4

[<Test>]
let ``Varn add``() =
    let ans = Varn.add vrn mvl1.Mvs
    let ans2 = Varn.add vrn mvl5.Mvs
    let ans3 = Varn.add vrn mvl6.Mvs
    ans.Lines.Length|>shouldEqual 4
    ans2.Lines.Length|>shouldEqual 5
    ans3.Lines.Length|>shouldEqual 4

[<Test>]
let ``Varn del``() =
    let ans = Varn.del vrn 0
    ans.Lines.Length|>shouldEqual 3
    (fun () -> Varn.del vrn 5 |> ignore)|>shouldFail<System.IndexOutOfRangeException>
    (fun () -> Varn.del vrn 6 |> ignore)|>shouldFail<System.IndexOutOfRangeException>

[<Test>]
let ``Varn mvl``() =
    let ans = Varn.mvl(vrn,0,3)
    ans.[0].Mpgn|>shouldEqual "d4"
    ans.Length|>shouldEqual 4

[<Test>]
let ``Varn wvars``() =
    let ans = Varn.wvars()
    ans.[0]|>shouldEqual "Benko"
    ans.Length|>shouldEqual 11

[<Test>]
let ``Varn bvars``() =
    let ans = Varn.bvars()
    ans.[0]|>shouldEqual "Abrahams"
    ans.Length|>shouldEqual 11

[<Test>]
let ``Varn cur2txt``() =
    let ans = Varn.cur2txt vrn
    ans.[0]|>shouldEqual "d2d4 d7d6 d4d5 e7e5 d5e6"

[<Test>]
let ``Varn save``() =
    let ans = Varn.save vrn
    ans|>shouldEqual "Save successful for variation: NotSet"
    let fn = Path.Combine(Varn.wfol,vrn.Name+".json")
    File.Exists(fn)|>shouldEqual true
    File.Delete(fn)
    File.Exists(fn)|>shouldEqual false

[<Test>]
let ``Varn load``() =
    let ans = Varn.save vrn
    ans|>shouldEqual "Save successful for variation: NotSet"
    let ans2 = Varn.load(vrn.Name,vrn.Isw)
    ans2|>shouldEqual vrn
    let fn = Path.Combine(Varn.wfol,vrn.Name+".json")
    File.Exists(fn)|>shouldEqual true
    Varn.delete(vrn.Name,vrn.Isw)
    File.Exists(fn)|>shouldEqual false
