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

    let scr10s = new System.Collections.Generic.Dictionary<string,int>()
    let scr25s = new System.Collections.Generic.Dictionary<string,int>()
    let bresps = new System.Collections.Generic.Dictionary<string,string>()

    let updscr10(nm,isw) =
        let var = load(nm,isw)
        let getline line =
            let pos = Pos.Start() 
            let getmv i (mv:Move) =
                Console.WriteLine(nm + " " + i.ToString() + " " + mv.Mpgn)
                mv|>pos.DoMv
                let fen = pos.ToString()
                if scr10s.ContainsKey(fen) then
                    let scr10 = scr10s.[fen]
                    if scr10=mv.Scr10 then mv
                    else
                        {mv with Scr10=scr10}
                elif mv.Scr10<>0 then 
                    scr10s.[fen]<-mv.Scr10
                    mv
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
                                |> List.map (fun (m : Move) -> m.UCI)
                                |> List.reduce (fun a b -> a + " " + b)

                    ComputeAnswer(mvstr, 10, procp)
                    procp.WaitForExit()
                    let nmv = {mv with Scr10=scr}
                    scr10s.[fen]<-scr
                    nmv
    
            let nmvs = line.Mvs|>List.mapi getmv
            let nline = {line with Mvs=nmvs}
            nline
    
        let nlines = var.Lines|>List.map getline
        let nvar = {var with Lines=nlines}
        nvar|>Varn.save|>ignore
    wvars()|>List.iter(fun nm -> updscr10(nm,true))
    bvars()|>List.iter(fun nm -> updscr10(nm,false))

    let updscr25(nm,isw) =
        let var = load(nm,isw)
        let getline line =
            let pos = Pos.Start() 
            let getmv i (mv:Move) =
                Console.WriteLine(nm + " " + i.ToString() + " " + mv.Mpgn)
                mv|>pos.DoMv
                let fen = pos.ToString()
                if scr25s.ContainsKey(fen) && bresps.ContainsKey(fen) then
                    let scr25 = scr25s.[fen]
                    let bresp = bresps.[fen]
                    if scr25=mv.Scr25 && bresp=mv.Bresp then mv
                    else
                        {mv with Scr25=scr25;Bresp=bresp}
                elif mv.Scr25<>0 || mv.Bresp<>"" then 
                    scr25s.[fen]<-mv.Scr25
                    bresps.[fen]<-mv.Bresp
                    mv
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
                                |> List.map (fun (m : Move) -> m.UCI)
                                |> List.reduce (fun a b -> a + " " + b)

                    ComputeAnswer(mvstr, 25, procp)
                    procp.WaitForExit()
                    let nmv = {mv with Scr25=scr;Bresp=br}
                    scr25s.[fen]<-scr
                    bresps.[fen]<-br
                    nmv
    
            let nmvs = line.Mvs|>List.mapi getmv
            let nline = {line with Mvs=nmvs}
            nline
    
        let nlines = var.Lines|>List.map getline
        let nvar = {var with Lines=nlines}
        nvar|>Varn.save|>ignore
    wvars()|>List.iter(fun nm -> updscr25(nm,true))
    bvars()|>List.iter(fun nm -> updscr25(nm,false))

    let updmve(nm,isw) =
        let var = load(nm,isw)
        let getline line =
            let getmv i (mv:Move) =
                Console.WriteLine(nm + " " + i.ToString() + " " + mv.Mpgn)
                if i=0 then mv
                else
                    let prevmv = line.Mvs.[i-1]
                    if prevmv.Bresp=mv.Mpgn then 
                        {mv with Meval=MvEval.Excellent}
                    elif abs(mv.Scr25-mv.Scr10)>20 then
                        {mv with Meval=MvEval.Surprising}
                    elif prevmv.Scr25+mv.Scr25>20 then
                        {mv with Meval=MvEval.Weak}
                    else
                        {mv with Meval=MvEval.Normal}
    
            let nmvs = line.Mvs|>List.mapi getmv
            let nline = {line with Mvs=nmvs}
            nline
    
        let nlines = var.Lines|>List.map getline
        let nvar = {var with Lines=nlines}
        nvar|>Varn.save|>ignore
    wvars()|>List.iter(fun nm -> updmve(nm,true))
    bvars()|>List.iter(fun nm -> updmve(nm,false))


    0 // return an integer exit code
