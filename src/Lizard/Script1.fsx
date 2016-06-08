#r"bin\\debug\\Lizard.exe"
open Lizard
let opts1 = Opts.load()
Opts.save({opts1 with Opnfol = "I:\\LizData\\Openings"})
let opts2 = Opts.load()
let dob (brch:Move list) =
    let pos = Pos.Start()
    for i = 0 to brch.Length-1 do
        let mv0 = brch.[i]
        let mvft = pos.GetMvFT(mv0.Mfrom,mv0.Mto)
        let strip chars = 
            String.collect (fun c -> 
                if Seq.exists ((=) c) chars then ""
                else string c)
        let m = mv0.Mpgn |> strip "+#=!?"
        let mv = {mv0 with Mpgn = m }
        if mv<>mvft then failwith ("no match: " + pos.ToString() + " mv: " + mv.Mpgn + " mvft: " + mvft.Mpgn + " from: " + mv.Mfrom.ToString() + " to: " + mv.Mto.ToString())
        pos.DoMv mv
let dov vrn = vrn.Brchs|>List.iter dob
let wvars = Varn.wvars()
let wvrns = wvars|>List.map(fun w -> Varn.load(w,true))
wvrns|>List.iter dov
let bvars = Varn.bvars()
let bvrns = bvars|>List.map(fun b -> Varn.load(b,false))
bvrns|>List.iter dov