namespace Lizard

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
        Send("setoption name Threads value " + (System.Environment.ProcessorCount - 1).ToString(), prc)
        Send("position startpos", prc)
        Send("position startpos moves " + ln + " ", prc)
        if (depth > 0) then Send("go depth " + depth.ToString(), prc)
        else Send("go movetime " + opts.Gsecpm.ToString() + "000", prc)
    
    ///set up process
    let SetUpPrc (prc : System.Diagnostics.Process) eng = 
        let opts = Opts.load()
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
    
    ///set game header given color
    let setghdr (w, b) (gm : PGN.Game) = 
        { gm with White = w
                  Black = b
                  Year = 
                      DateTime.Now.Year
                      |> Convert.ToInt16
                      |> Some
                  Month = 
                      DateTime.Now.Month
                      |> Convert.ToByte
                      |> Some
                  Day = 
                      DateTime.Now.Day
                      |> Convert.ToByte
                      |> Some }
    
    ///update PGN given position header and name
    let updPGN gm nm = 
        let opts = Opts.load()
        let pgnfil = Path.Combine(opts.Gmfol, nm)
        Directory.CreateDirectory opts.Gmfol |> ignore
        File.AppendAllText(pgnfil, Environment.NewLine + gm.ToString())
        MessageBox.Show("Game added to " + pgnfil, "Game Saved", MessageBoxButtons.OK, MessageBoxIcon.Information) 
        |> ignore
