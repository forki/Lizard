module Ref

open FsUnit
open NUnit.Framework
open Lizard.Ref

[<Test>]
let ``Ref f 45``() =
     f.[45]|> should equal "f"
[<Test>]
let ``Ref r 45``() =
     r.[45]|> should equal "6"
[<Test>]
let ``Ref sq 45``() =
     sq.[45]|> should equal "f6"
[<Test>]
let ``Ref fi 45``() =
     fi.[45]|> should equal 5
[<Test>]
let ``Ref ri 45``() =
     ri.[45]|> should equal 5
[<Test>]
let ``Ref GetOrdFR 45``() =
     GetOrdFR(5,5)|> should equal 45
[<Test>]
let ``Ref ord 45``() =
     ord('f','6')|> should equal 45




