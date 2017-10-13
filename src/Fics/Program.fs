//read FICS data in XML format
open FSharp.Data
open Lizard
open Lizard.Varn

type FICS = XmlProvider<"explorer.cgi.xml">

[<EntryPoint>]
let main argv = 

    let updfics(nm,isw) =
        let var = load(nm,isw)
        let getline il line =
            System.Console.WriteLine(nm + " on line: " + il.ToString())
            let mutable fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1"
            let mutable fics = FICS.Load("http://www.ficsgames.org/cgi-bin/explorer.cgi?FEN=" + fen)
            let mutable eco = ""
            let mutable fin = false
            let getmv i (mv:Move1) =
                if mv.ECO<>"" then mv
                elif fin then {mv with ECO=eco;FicsPc=0.0}
                else
                    let numgames = fics.NumGames
                    let pgn = mv.Mpgn
                    let fmvs = fics.MvList
                    if fmvs.Length=0 then 
                        fin <- true
                        {mv with ECO=eco;FicsPc=0.0}
                    else
                        let fmvl = (fmvs|>Array.filter(fun m -> m.San=pgn))
                        if fmvl.Length=0 then
                            fin <- true
                            {mv with ECO=eco;FicsPc=0.0}
                        else
                            let fmv = fmvl.[0]
                            let mvnum = fmv.N
                            let fpct = if numgames=0 then 0.0 else float(mvnum)/float(numgames)
                            try
                                fen <- fmv.F
                            with _ -> fin <- true
                            fics <- FICS.Load("http://www.ficsgames.org/cgi-bin/explorer.cgi?FEN=" + fen)
                            try
                                eco <- fics.Eco + " - " + fics.EcoName
                            with _ ->()
                            let nmv = {mv with ECO=eco;FicsPc=fpct}
                            nmv
            let nmvs = line.Mvs|>List.mapi getmv
            let nline = {line with Mvs=nmvs}
            nline

        let nlines = var.Brchs|>List.mapi getline
        let nvar = {var with Brchs=nlines}
        nvar|>Varn.save|>ignore


    wvars()|>List.iter(fun nm -> updfics(nm,true))
    bvars()|>List.iter(fun nm -> updfics(nm,false))





    0 // return an integer exit code
