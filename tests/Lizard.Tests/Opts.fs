module Opts

open FsUnit
open NUnit.Framework
open Lizard
open System.IO

[<Test>]
let ``Opts optfile``() =
    let ans = Opts.optfile
    ans|>Path.GetFileName|>should equal "Options.json"

[<Test>]
let ``Opts contents``() =
    let ans = Opts.load()
    ans.Opnfol|>should equal "I:\\LizData\\Openings"
