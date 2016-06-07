#r"bin\\debug\\Lizard.exe"
open Lizard
let opts1 = Opts.load()
Opts.save({opts1 with Opnfol = "I:\\LizData\\Openings"})
let opts2 = Opts.load()
let vrn = Varn.load("Benko",true)
let brch = vrn.Brchs.[0]
let pos = Pos.Start()
for i = 0 to brch.Length-1 do
    let mv = brch.[i]
    let mvft = pos.GetMvFT(mv.Mfrom,mv.Mto)
    if mv<>mvft then failwith ("no match: " + pos.ToString() + " mv: " + mv.Mpgn + " mvft: " + mvft.Mpgn)
    pos.DoMv mv