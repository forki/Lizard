module Ref

open FsUnitTyped
open NUnit.Framework
open Lizard.Ref

[<Test>]
let ``Ref f 45``() =
     f.[45]|> shouldEqual "f"
[<Test>]
let ``Ref r 45``() =
     r.[45]|> shouldEqual "3"
[<Test>]
let ``Ref sq 45``() =
     sq.[45]|> shouldEqual "f3"
[<Test>]
let ``Ref fi 45``() =
     fi.[45]|> shouldEqual 5
[<Test>]
let ``Ref ri 45``() =
     ri.[45]|> shouldEqual 2
[<Test>]
let ``Ref GetOrdFR 45``() =
     GetOrdFR(5,2)|> shouldEqual 45
[<Test>]
let ``Ref ord 45``() =
     ord('f','3')|> shouldEqual 45




