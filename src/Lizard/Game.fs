namespace LizardChess

open System
open System.IO
open System.Windows.Forms

module Game = 
    ///log engine activity
    let Log(message : string) = 
        let opts = Opts.load()
        if (opts.Elog) then 
            use w = new StreamWriter(Eng.logfile, true)
            w.WriteLine(message)
            w.Close()
    
    ///send message to engine
    let Send(command, prc : System.Diagnostics.Process) = 
        let opts = Opts.load()
        if (opts.Elog) then Log(command)
        prc.StandardInput.WriteLine(command)
    
    ///set up engine
    let ComputeAnswer(ln, depth, prc) = 
        let opts = Opts.load()
        Send("ucinewgame", prc)
        Send
            ("setoption name Threads value " 
             + (System.Environment.ProcessorCount - 1).ToString(), prc)
        Send("position startpos", prc)
        Send("position startpos moves " + ln + " ", prc)
        if (depth > 0) then Send("go depth " + depth.ToString(), prc)
        else Send("go movetime " + opts.Gsecpm.ToString() + "000", prc)
    
    ///set up process
    let SetUpPrc (prc : System.Diagnostics.Process) eng = 
        let opts = Opts.load()
        prc.StartInfo.CreateNoWindow <- true
        prc.StartInfo.FileName <- Eng.enginepath opts eng
        prc.StartInfo.WorkingDirectory <- Eng.efol
        prc.StartInfo.RedirectStandardOutput <- true
        prc.StartInfo.UseShellExecute <- false
        prc.StartInfo.RedirectStandardInput <- true
        prc.StartInfo.WindowStyle <- System.Diagnostics.ProcessWindowStyle.Hidden
        prc.Start() |> ignore
        prc.BeginOutputReadLine()
    
    ///get game header given color
    let getghdr isw eng = 
        let opts = Opts.load()
        { blankhdr with White = 
                            (if isw then "Me"
                             else eng)
                        Black = 
                            (if not isw then "Me"
                             else eng)
                        Date = DateTime.Now.ToString("yyyy.MM.dd") }
    
    ///update PGN given position header and name
    let updPGN pos (hdr : Gmhdr) nm = 
        let nl = Environment.NewLine
        let opts = Opts.load()
        let pgnfil = Path.Combine(opts.Gmfol, nm)
        let hdrstr = 
            "[Event \"" + hdr.Event + "\"]" + nl + "[Site \"" + hdr.Site + "\"]" 
            + nl + "[Date \"" + hdr.Date + "\"]" + nl + "[Round \"" + hdr.Round 
            + "\"]" + nl + "[White \"" + hdr.White + "\"]" + nl + "[Black \"" 
            + hdr.Black + "\"]" + nl + "[Result \"" + hdr.Result.ToString() 
            + "\"]" + nl
        let mvsPgn = Posn.psn2pgn pos
        Directory.CreateDirectory opts.Gmfol |> ignore
        File.AppendAllText(pgnfil, hdrstr + mvsPgn)
        MessageBox.Show
            ("Game added to " + pgnfil, "Game Saved", MessageBoxButtons.OK, 
             MessageBoxIcon.Information) |> ignore