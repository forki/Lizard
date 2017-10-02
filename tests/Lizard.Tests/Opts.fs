module Opts

open FsUnitTyped
open NUnit.Framework
open Lizard
open System.IO

[<Test>]
let ``Opts optfile``() =
    let ans = Opts.optfile
    ans|>Path.GetFileName|>shouldEqual "Options.json"

[<Test>]
let ``Opts contents``() =
    let ans = Opts.load()
    ans.Opnfol|>shouldEqual "I:\\LizData\\Openings"
