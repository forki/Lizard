//updates moves using engine
open System
open System.IO
open Lizard.Varn
open Lizard

[<EntryPoint>]
let main argv = 
    ///send message to engine
    let Send(command:string, prc : System.Diagnostics.Process) = 
        prc.StandardInput.WriteLine(command)
    
    ///set up engine
    let ComputeAnswer(ln, depth, prc) = 
        Send("ucinewgame", prc)
        Send("setoption name Threads value " + (System.Environment.ProcessorCount - 1).ToString(), prc)
        Send("position startpos", prc)
        Send("position startpos moves " + ln + " ", prc)
        Send("go depth " + depth.ToString(), prc)
    
    ///set up process
    let SetUpPrc (prc : System.Diagnostics.Process) eng = 
        prc.StartInfo.CreateNoWindow <- true
        prc.StartInfo.FileName <- "stockfish.exe"
        prc.StartInfo.WorkingDirectory <- Path.GetDirectoryName
                                              (System.Reflection.Assembly.GetExecutingAssembly().Location)
        prc.StartInfo.RedirectStandardOutput <- true
        prc.StartInfo.UseShellExecute <- false
        prc.StartInfo.RedirectStandardInput <- true
        prc.StartInfo.WindowStyle <- System.Diagnostics.ProcessWindowStyle.Hidden
        prc.Start() |> ignore
        prc.BeginOutputReadLine()

    let updscr10(nm,isw) =
        let var = load(nm,isw)
        let getline line =
            let getmv i (mv:Move1) =
                if mv.Scr10<>0 then mv
                else
                    let procp = new System.Diagnostics.Process()
                    let eng = "stockfish.exe"
                    let mutable msg = ""
                    let mutable scr = 0
                    //p_out
                    let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
                        if not (e.Data = null || e.Data = "") then 
                            msg <- e.Data.ToString().Trim()
                            if msg.StartsWith("info depth") then
                                let bits = msg.Split([|' '|])
                                let p = Array.IndexOf(bits,"cp")
                                if p<> -1 then scr <- int(bits.[p+1])
                            if msg.StartsWith("bestmove") then procp.Kill()
                    procp.OutputDataReceived.Add(pOut)
                    //Start process
                    SetUpPrc procp eng
                    // call calcs
                    // need to send game position moves as UCI
                    let mvstr = line.Mvs.[0..i]
                                |> List.map (fun (m : Move1) -> m.UCI)
                                |> List.reduce (fun a b -> a + " " + b)

                    ComputeAnswer(mvstr, 10, procp)
                    procp.WaitForExit()
                    let nmv = {mv with Scr10=scr}
                    nmv
    
            let nmvs = line.Mvs|>List.mapi getmv
            let nline = {line with Mvs=nmvs}
            nline
    
        let nlines = var.Brchs|>List.map getline
        let nvar = {var with Brchs=nlines}
        nvar|>Varn.save|>ignore
    //wvars()|>List.iter(fun nm -> updscr10(nm,true))
    //bvars()|>List.iter(fun nm -> updscr10(nm,false))

    let updscr25(nm,isw) =
        let var = load(nm,isw)
        let getline line =
            let pos = Pos.Start() 
            let getmv i (mv:Move1) =
                Console.WriteLine(nm + " " + i.ToString() + " " + mv.Mpgn)
                mv|>pos.DoMv
                if mv.Scr25<>0 || mv.Bresp<>"" then mv
                else
                    let procp = new System.Diagnostics.Process()
                    let eng = "stockfish.exe"
                    let mutable msg = ""
                    let mutable scr = 0
                    let mutable br = ""
                    //p_out
                    let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
                        if not (e.Data = null || e.Data = "") then 
                            msg <- e.Data.ToString().Trim()
                            if msg.StartsWith("info depth") then
                                let bits = msg.Split([|' '|])
                                let p = Array.IndexOf(bits,"cp")
                                if p<> -1 then scr <- int(bits.[p+1])
                            if msg.StartsWith("bestmove") then 
                                let bits = msg.Split([|' '|])
                                let bruci = bits.[1] 
                                let brmv = bruci|>pos.GetMvUCI
                                br <- brmv.Mpgn
                                procp.Kill()
                    procp.OutputDataReceived.Add(pOut)
                    //Start process
                    SetUpPrc procp eng
                    // call calcs
                    // need to send game position moves as UCI
                    let mvstr = line.Mvs.[0..i]
                                |> List.map (fun (m : Move1) -> m.UCI)
                                |> List.reduce (fun a b -> a + " " + b)

                    ComputeAnswer(mvstr, 25, procp)
                    procp.WaitForExit()
                    let nmv = {mv with Scr25=scr;Bresp=br}
                    nmv
    
            let nmvs = line.Mvs|>List.mapi getmv
            let nline = {line with Mvs=nmvs}
            nline
    
        let nlines = var.Brchs|>List.map getline
        let nvar = {var with Brchs=nlines}
        nvar|>Varn.save|>ignore
    wvars()|>List.iter(fun nm -> updscr25(nm,true))
    bvars()|>List.iter(fun nm -> updscr25(nm,false))



    0 // return an integer exit code
