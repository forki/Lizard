namespace LizardChessUI

open LizardChess
open System
open System.IO
open System.Net.Sockets
open System.Threading
open System.Windows.Forms
open System.Text

type Mode = 
    | DoVarn
    | DoTest
    | DoPlay
    | DoFics
    | DoDb

type PosnState(sst:SharedState) = 
    let mutable pos = Posn.st
    let mutable psmvs = []
    let mutable cfdt = FcsDt.empfdb
    let mutable canl = Eng.empanl
    //Events
    let pchngEvt = new Event<_>()
    let bmchngEvt = new Event<_>()
    let promEvt = new Event<_>()
    let psmvsEvt = new Event<_>()
    let orntEvt = new Event<_>()
    let fdtchngEvt = new Event<_>()
    //publish
    member x.PosChng = pchngEvt.Publish
    member x.BmChng = bmchngEvt.Publish
    member x.Prom = promEvt.Publish
    member x.PsMvs = psmvsEvt.Publish
    member x.Ornt = orntEvt.Publish
    member x.FdtChng = fdtchngEvt.Publish
    //members
    member x.CurPos = pos
    member x.SetPos ps =
        pos <- ps
        pos |> pchngEvt.Trigger
    member x.SetCanl() =
        canl <- pos
                |> Posn.psn2str
                |> Eng.getanl (Eng.loadLineStore())
        canl |> bmchngEvt.Trigger
    member x.SetCfdt() =
        cfdt <- pos
                |> Posn.psn2str
                |> FcsDt.getfdb (FcsDt.loadFicsDbStore())
        cfdt |> fdtchngEvt.Trigger
    member x.SetPsmvs pms =
        psmvs <- pms
        psmvs |> psmvsEvt.Trigger
    member x.Move(mfrom, mto) = 
        if psmvs.Length > 0 then 
            let mvl = psmvs |> List.filter (fun m -> m.To = mto)
            if mvl.Length = 1 then 
                let mv = mvl.[0]
                x.SetPos(Posn.DoMove(mv, pos))
                x.SetCanl()
                x.SetCfdt()
                x.SetPsmvs []
                sst.DoMode()
            elif mvl.Length > 1 then mvl |> promEvt.Trigger
            else 
                pos |> pchngEvt.Trigger
                x.SetCanl()
                x.SetCfdt()
                x.SetPsmvs []
    member x.Promote(mv) = 
        x.SetPos(Posn.DoMove(mv, pos))
        x.SetCanl()
        x.SetCfdt()
        sst.DoMode()
    member x.GetPossMvs(mfrom) = 
        let pcl = pos.Pcs |> List.filter (fun pc -> pc.Sq = mfrom)
        if pcl.Length > 0 && pcl.[0].IsW = pos.IsWhite then 
            x.SetPsmvs(Posn.GenLegalMvs(pcl.[0], pos))
    member x.TrigOri(isw) = isw|>orntEvt.Trigger

and VarnState(sst:SharedState) =
    let mutable wvrs = Varn.wvars()
    let mutable bvrs = Varn.bvars()
    let mutable visw = true
    let mutable curv = 
        if visw then
            if wvrs.Length>1 then Varn.load (wvrs.[0], visw) else Varn.emp
        else
            if bvrs.Length>1 then Varn.load (bvrs.[0], visw) else Varn.emp


//        Varn.load ((if visw then wvrs.[0]
//                    else bvrs.[0]), visw)
    let mutable selvar = -1
    //Events
    let vchngEvt = new Event<_>()
    let cchngEvt = new Event<_>()
    let selCelEvt = new Event<_>()
    //publish
    member x.VarsChng = vchngEvt.Publish
    member x.CurChng = cchngEvt.Publish
    member x.SelCell = selCelEvt.Publish
    //members
    member x.CurVarn = curv
    member x.SetVarn cv = curv <- cv
    member x.SelVar = selvar
    member x.SetVar sv = selvar <- sv
    member x.VarIsw = visw
    member x.SetIsw isw = visw <- isw
    member x.Vars(isw) = 
        if isw then wvrs
        else bvrs
    member x.GetVarn() = Varn.lines curv
    member x.GetPos(vr, mv) = 
        let mvl = Varn.mvl (curv, vr, mv)
        selvar <- vr
        let pstt:PosnState = sst.Pstt
        pstt.SetPos(Posn.DoMoves(mvl, Posn.st)) 
        pstt.SetCanl()
        pstt.SetCfdt()
    member x.OpenVarn(nm, isw) = 
        let pstt:PosnState = sst.Pstt
        sst.SetMode(DoVarn)
        visw <- isw
        curv <- Varn.load (nm, visw)
        let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
        (curv |> Varn.lines, currAnls) |> cchngEvt.Trigger
        visw |> pstt.TrigOri
    member x.SaveVarn() = Varn.save (curv)
    member x.NewVarn(nm, isw) = 
        let pstt:PosnState = sst.Pstt
        sst.SetMode(DoVarn)
        curv <- Varn.cur (nm, isw)
        let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
        (curv |> Varn.lines, currAnls) |> cchngEvt.Trigger
        visw |> pstt.TrigOri
        if isw then wvrs <- nm :: wvrs
        else bvrs <- nm :: bvrs
        visw <- isw
        visw |> vchngEvt.Trigger
    member x.SaveAsVarn(nm) = 
        curv <- Varn.saveas (curv, nm)
        let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
        (curv |> Varn.lines, currAnls) |> cchngEvt.Trigger
        if visw then wvrs <- nm :: wvrs
        else bvrs <- nm :: bvrs
        visw |> vchngEvt.Trigger
    member x.DelVarn(nm, isw) = 
        Varn.delete (nm, isw)
        if isw then wvrs <- Varn.wvars()
        else bvrs <- Varn.bvars()
        visw |> vchngEvt.Trigger
    member x.DelLine() = 
        curv <- Varn.del curv selvar
        selvar <- -1
        let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
        (curv |> Varn.lines, currAnls) |> cchngEvt.Trigger
    member x.GetNextMvs() = 
        let pstt:PosnState = sst.Pstt
        let pos = pstt.CurPos
        Varn.findnmvs pos curv.Brchs
    member x.DoNextMv(mv) = 
        let pstt:PosnState = sst.Pstt
        let pos = pstt.CurPos
        let move = Posn.FndMv(mv, pos)
        if move.IsSome then 
            pstt.SetPos(Posn.DoMove(move.Value, pos)) 
            pstt.SetCanl()
            pstt.SetCfdt()
            let pos = pstt.CurPos
            let oselvar = Varn.findsv pos curv.Brchs
            if oselvar.IsSome then 
                selvar <- oselvar.Value
                let selmv = pos.Mhst.Length - 1
                (selvar, selmv) |> selCelEvt.Trigger
    member x.TrigCurv(cc) = cc|>cchngEvt.Trigger
    member x.TrigSelv(ss) = ss|>selCelEvt.Trigger

and TestState(sst:SharedState) =
    //Test
    let mutable tests : TestDet [] = [||]
    let mutable numtst = -1
    let mutable tstsdone = 0
    let mutable tstscor = 0
    let mutable tnm = ""
    let mutable tisw = false
    let mutable trnd = false
    //Events
    //test change
    let tchgEvt = new Event<_>()
    let tresEvt = new Event<_>()
    //test results
    let resTabEvt = new Event<_>()
    let resLoadEvt = new Event<_>()
    //publish
    member x.TestChng = tchgEvt.Publish
    member x.TestRes = tresEvt.Publish
    member x.ResTabLoad = resTabEvt.Publish
    member x.ResLoad = resLoadEvt.Publish
    //members
    member x.Tests = tests
    member x.SetTest(i,t) = tests.[i] <- t
    member x.Num = numtst
    member x.Done = tstsdone 
    member x.SetDone td = tstsdone <- td 
    member x.Cor = tstscor 
    member x.SetCor tc = tstscor <- tc 
    member x.LoadTest(rnd, nm, isw) = 
        System.Windows.Forms.Cursor.Current <- System.Windows.Forms.Cursors.WaitCursor
        sst.SetMode(DoTest)
        tests <- if rnd then Test.fromName nm isw
                 else Test.fromNameLin nm isw
        tstsdone <- 0
        tstscor <- 0
        tnm <- nm
        tisw <- isw
        trnd <- rnd
        tests |> tchgEvt.Trigger
    member x.SetTestPos(i) = 
        let pstt:PosnState = sst.Pstt
        if numtst <> i then 
            pstt.SetPos(Test.GetPosn(tests.[i]))
            numtst <- i
    member x.CloseTest() = 
        let results = 
            if trnd then Test.loadResults()
            else Test.loadResultsLin()
        if tstsdone > 0 then 
            let res = Test.createres (tnm, tisw, (100 * tstscor) / tstsdone)
            if trnd then Test.saveRes (results, res)
            else Test.saveResLin (results, res)
        tests <- [||]
        numtst <- -1
        tstsdone <- 0
        tstscor <- 0
        tnm <- ""
        tisw <- false
        trnd <- false
        sst.SetMode(DoVarn)
    //Test Results
    member x.ShowRes(rnd) = 
        resTabEvt.Trigger()
        let res = 
            if rnd then Test.getallres()
            else Test.getallreslin()
        res |> resLoadEvt.Trigger
    member x.TrigRes(rs) = rs|>tresEvt.Trigger

and AnalState(sst:SharedState) =
    //Analyse
    let mutable isanl = false
    let mutable proc = new System.Diagnostics.Process()
    let mutable procp = new System.Diagnostics.Process()
    let mutable ln = ""
    let mutable lnct = 0
    let mutable mvct = 0
    let mutable dpth = 0
    let mutable lastmsg = ""
    let mutable lastmsg1 = ""
    let mutable lastmsg2 = ""
    //Events
    let achngEvt = new Event<_>()
    let ahchngEvt = new Event<_>()
    let amsgEvt = new Event<_>()
    let apchngEvt = new Event<_>()
    let ahpchngEvt = new Event<_>()
    let apmsgEvt = new Event<_>()
    //main recursive function
    let rec getAnswer(processBM, answer) linestore (vn:string[]) (opts, eng, nm) = 
        let send msg = Game.Send(msg, proc)
        if (processBM) then 
            Eng.procbm(linestore, ln, mvct, answer,[ lastmsg; lastmsg1; lastmsg2 ])
        Eng.saveLineStore (linestore)
        let lnctnew = 
            if mvct = ln.Trim().Split(' ').Length then lnct + 1
            else -1
        if lnctnew = vn.Length then 
            if (dpth = opts.Emaxdepth) then 
                send("stop")
                send("")
                let text = 
                    "Finished processing variation " + nm + " to depth " 
                    + dpth.ToString()
                text |> ahchngEvt.Trigger
            else 
                ln <- vn.[0].Trim()
                lnct <- 0
                let mvs = ln.Split(' ')
                mvct <- min (mvs.Length - 1) 5
                dpth <- dpth + 1
                //clear for next line
                Eng.hdr(eng, ln, lnct, mvct, dpth) |> ahchngEvt.Trigger
                if (Eng.alreadyDone 
                        (linestore, Eng.str2str (ln, mvct), dpth)) then 
                    getAnswer(false, "") linestore vn (opts, eng, nm)
                else Game.ComputeAnswer(Eng.str2str (ln, mvct), dpth, proc)
        else 
            if lnctnew <> -1 then 
                let mvs = vn.[lnctnew].Trim().Split(' ')
                mvct <- min (mvs.Length - 1) 5
                ln <- vn.[lnctnew].Trim()
                lnct <- lnctnew
            else mvct <- mvct + 1
            //clear for next line
            Eng.hdr(eng, ln, lnct, mvct, dpth) |> ahchngEvt.Trigger
            //do next one
            if (Eng.alreadyDone (linestore, Eng.str2str (ln, mvct), dpth)) then 
                getAnswer(false, "") linestore vn (opts, eng, nm)
            else Game.ComputeAnswer(Eng.str2str (ln, mvct), dpth, proc)
    //publish
    member x.AnlChng = achngEvt.Publish
    member x.AnlHeadChng = ahchngEvt.Publish
    member x.AnlMsg = amsgEvt.Publish
    member x.AnlpChng = apchngEvt.Publish
    member x.AnlpHeadChng = ahpchngEvt.Publish
    member x.AnlpMsg = apmsgEvt.Publish
    //members
    member x.CurrBms = 
        let vstt:VarnState = sst.Vstt
        let curv = vstt.CurVarn
        let strs = Varn.cur2txt (curv)
        let vnpsns = curv.Brchs
        Seq.map2 (Eng.strmvl2bms (Eng.loadLineStore())) strs vnpsns 
        |> Seq.toArray
    
    //Analyse Line
    member x.AnlStart(nm, isw) = 
        proc <- new System.Diagnostics.Process()
        let vn = Varn.loadtxta (nm, isw)
        let linestore = Eng.loadLineStore()
        let opts = Opts.load()
        let eng = opts.Eng
        //p_out
        let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
            if not (e.Data = null || e.Data = "") then 
                let line = e.Data.ToString().Trim()
                if (opts.Elog) then Game.Log("--> " + line)
                lastmsg2 <- lastmsg1
                lastmsg1 <- lastmsg
                lastmsg <- line
                // ANALYZE THE ANSWER...
                if (line.StartsWith("bestmove")) then 
                    (lastmsg2, Eng.str2str (ln, mvct), dpth) |> amsgEvt.Trigger
                    (lastmsg1, Eng.str2str (ln, mvct), dpth) |> amsgEvt.Trigger
                    (line, Eng.str2str (ln, mvct), dpth) |> amsgEvt.Trigger
                    let token = line.Split([| ' ' |])
                    if token.Length > 1 then getAnswer(true, token.[1]) linestore vn (opts, eng, nm)
        
        proc.OutputDataReceived.Add(pOut)
        //Start process
        Game.SetUpPrc proc eng
        //set mutables
        ln <- vn.[0].Trim()
        let mvs = ln.Split(' ')
        lnct <- 0
        mvct <- min (mvs.Length - 1) 5
        dpth <- opts.Emindepth
        // call calcs
        if (Eng.alreadyDone (linestore, Eng.str2str (ln, mvct), dpth)) then 
            getAnswer(false, "") linestore vn (opts, eng, nm)
        else Game.ComputeAnswer(Eng.str2str (ln, mvct), dpth, proc)
        isanl <- true
        isanl |> achngEvt.Trigger
        Eng.hdr(eng, ln, lnct, mvct, dpth) |> ahchngEvt.Trigger
    
    member x.AnlStop() = 
        if proc <> null then proc.Kill()
        isanl <- false
        isanl |> achngEvt.Trigger
        "Stopped" |> ahchngEvt.Trigger
    
    //Analyse Pos
    member x.AnlpStart() = 
        procp <- new System.Diagnostics.Process()
        let opts = Opts.load()
        let eng = opts.Eng
        let send msg = Game.Send(msg, procp)
        
        //p_out
        let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
            if not (e.Data = null || e.Data = "") then 
                let msg = e.Data.ToString().Trim()
                msg |> apmsgEvt.Trigger
        procp.OutputDataReceived.Add(pOut)
        //Start process
        Game.SetUpPrc procp eng
        // call calcs
        // need to send game position moves as UCI
        let pstt = sst.Pstt
        let pos = pstt.CurPos
        Game.ComputeAnswer(Posn.psn2str pos, 99, procp)
        isanl <- true
        isanl |> apchngEvt.Trigger
        (eng + " - " + (Posn.psn2str pos)) |> ahpchngEvt.Trigger
    
    member x.AnlpStop() = 
        if procp <> null then procp.Kill()
        isanl <- false
        isanl |> apchngEvt.Trigger
        "Stopped" |> ahpchngEvt.Trigger

and GameState(sst:SharedState) =
    let mutable gm = ""
    let nl = System.Environment.NewLine
    //Fics
    let mutable _socket : Socket = null
    let mutable _inBuffer : byte [] = null
    let mutable result = ""
    //TODO
    let mutable ghdr = ()//blankhdr
    //Database
    let mutable pgngms = []
    let mutable pgnmvs = [||]
    //Events
    //game
    let gmchngEvt = new Event<_>()
    let tosqEvt = new Event<_>()
    let gachngEvt = new Event<_>()
    //fics
    let fsttEvt = new Event<_>()
    let fendEvt = new Event<_>()
    let fmsgEvt = new Event<_>()
    let ftimEvt = new Event<_>()
    //db
    let dbgmldEvt = new Event<_>()
    let dbldEvt = new Event<_>()
    //publish
    member x.GameChng = gmchngEvt.Publish
    member x.GameAnlChng = gachngEvt.Publish
    member x.GameSqTo = tosqEvt.Publish
    member x.FicsStart = fsttEvt.Publish
    member x.FicsEnd = fendEvt.Publish
    member x.FicsMsg = fmsgEvt.Publish
    member x.FicsTim = ftimEvt.Publish
    member x.DbLoad = dbldEvt.Publish
    member x.DbGameLoad = dbgmldEvt.Publish
    //members
    member x.Gm = gm
    member x.SetGm g =  gm <- g
    member x.Ghdr = ghdr
    member x.SetGhdr gh =  ghdr <- gh
    member x.NewGame(isw) = 
        let pstt = sst.Pstt
        let vstt = sst.Vstt
        pstt.SetPos(Posn.st)
        vstt.SetIsw(isw)
        isw |> pstt.TrigOri
        sst.SetMode(DoPlay)
        let opts = Opts.load()
        //ghdr <- Game.getghdr isw opts.Geng
        gm <- ""
        (gm, ghdr) |> gmchngEvt.Trigger
        let uopn = opts.Guseopn
        if uopn then vstt.SetVarn(Varn.loada ("<All>", isw))
        let curv = vstt.CurVarn
        if isw && uopn then 
            let pos = pstt.CurPos
            let mvl = Varn.findnmvs pos curv.Brchs
            if mvl.Length > 0 then 
                pstt.SetPos(Posn.DoMove(mvl.Head, pos))
                let pos = pstt.CurPos
                [ mvl.Head ] |> tosqEvt.Trigger
                gm <- gm + "1. " + pos.Mhst.Head.PGN + " "
                (gm, ghdr) |> gmchngEvt.Trigger
                x.AnlPos()
        else 
            if not isw then x.AnlPos()
    member x.AnlPos() = 
        let prc = new System.Diagnostics.Process()
        let opts = Opts.load()
        let eng = opts.Geng
        let sec = opts.Gsecpm
        let uopn = opts.Guseopn
        let send msg = Game.Send(msg, prc)
        
        let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
            if not (e.Data = null || e.Data = "") then 
                let line = e.Data.ToString().Trim()
                if (line.StartsWith("bestmove")) then 
                    let bits = line.Split([| ' ' |])
                    let pstt = sst.Pstt
                    let pos = pstt.CurPos
                    let bm = Posn.FndMv(bits.[1], pos)
                    pstt.SetPos(Posn.DoMove(bm.Value, pos))
                    let pos = pstt.CurPos
                    [ bm.Value ] |> tosqEvt.Trigger
                    prc.Kill()
                    let mvstr = pos.Mhst.Head.PGN
                    let vstt = sst.Vstt
                    let visw = vstt.VarIsw
                    gm <- gm 
                          + (if visw then ""
                             else (pos.Mhst.Length / 2 + 1).ToString() + ". ") 
                          + mvstr + " "
//                    if mvstr.EndsWith("#") then 
//                        ghdr <- { ghdr with Result = if visw then Bwin else Wwin }
                    (gm, ghdr) |> gmchngEvt.Trigger
                    if uopn then 
                        let curv = vstt.CurVarn
                        let mvl = Varn.findnmvs pos curv.Brchs
                        if mvl.Length > 0 then 
                            let pos = pstt.CurPos
                            pstt.SetPos(Posn.DoMove(mvl.Head, pos))
                            let pos = pstt.CurPos
                            gm <- gm 
                                  + (if not visw then ""
                                     else (pos.Mhst.Length / 2 + 1).ToString() 
                                         + ". ") + pos.Mhst.Head.PGN + " "
                            (gm, ghdr) |> gmchngEvt.Trigger
                            x.AnlPos()
                else line |> gachngEvt.Trigger
        prc.OutputDataReceived.Add(pOut)
        Game.SetUpPrc prc eng
        let pstt = sst.Pstt
        let pos = pstt.CurPos
        Game.ComputeAnswer(Posn.psn2str pos, -1, prc)
    member x.FicsSend(msg) = 
        let rec onSend(ar : IAsyncResult) = 
            _socket <- ar.AsyncState :?> Socket
            try 
                let bytesSent = _socket.EndSend(ar)
                ()
            with ex -> 
                MessageBox.Show(ex.Message, "Error processing send buffer!")|> ignore
        try 
            let sendBytes = Encoding.ASCII.GetBytes(msg + "\n")
            let onsend = new AsyncCallback(onSend)
            _socket.BeginSend(sendBytes, 0, sendBytes.Length, SocketFlags.None, onsend, _socket) |> ignore
        with ex -> 
            MessageBox.Show("Setup receive callback failed: " + ex.ToString()) 
            |> ignore
    member x.DoFicsW(opts,buf) =
        let pstt = sst.Pstt
        let vstt = sst.Vstt
        pstt.SetPos(Posn.st)
        vstt.SetIsw(true)
        true |> pstt.TrigOri
        fsttEvt.Trigger()
        ghdr <- Fics.players (buf)
        gm <- ""
        (gm, ghdr) |> gmchngEvt.Trigger
        let uopn = opts.Guseopn
        if uopn then vstt.SetVarn(Varn.loada ("<All>", true))
        if uopn then 
            let pos = pstt.CurPos
            let curv = vstt.CurVarn
            let mvl = Varn.findnmvs pos curv.Brchs
            if mvl.Length > 0 then 
                pstt.SetPos(Posn.DoMove(mvl.Head, pos))
                let pos = pstt.CurPos
                gm <- gm + "1. " + pos.Mhst.Head.PGN + " "
                (gm, ghdr) |> gmchngEvt.Trigger
                //remove promotion as FICS defaults to Q unless you send "promote n"
                //see http://www.freechess.org/Help/HelpFiles/promote.html
                x.FicsSend(mvl.Head.UCI.Substring(0, 4))
        true
    member x.DoFicsB(opts,buf) =
        let pstt = sst.Pstt
        let vstt = sst.Vstt
        pstt.SetPos(Posn.st)
        vstt.SetIsw(false)
        false |> pstt.TrigOri
        fsttEvt.Trigger()
        ghdr <- Fics.players (buf)
        gm <- ""
        (gm, ghdr) |> gmchngEvt.Trigger
        let uopn = opts.Guseopn
        if uopn then vstt.SetVarn(Varn.loada ("<All>", false))
        true
    member x.DoFicsEnd(buf:string) =
        result <- buf.Split([| '}' |]).[1].Substring(1, 3)
//        ghdr <- { ghdr with Result = 
//                                if result = "1-0" then Wwin
//                                elif result = "0-1" then Bwin
//                                else Draw }
        (gm, ghdr) |> gmchngEvt.Trigger
        fendEvt.Trigger()
        false
    member x.DoFicsMove(opts,buf) =
        let pstt = sst.Pstt
        let vstt = sst.Vstt
        let wtm, btm, mv, misw = Fics.tmMove (buf)
        (wtm, btm, misw) |> ftimEvt.Trigger
        let visw = vstt.VarIsw
        if misw = visw then
        //TODO 
//            let pgnmv = Lizard.PGN.PgnParser.getpgnmv mv
//            let pos = pstt.CurPos
//            let fmv = Posn.pgn2mov pos pgnmv
//            pstt.SetPos(Posn.DoMove(fmv, pos))
            let pos = pstt.CurPos
//            [ fmv ] |> tosqEvt.Trigger
            let mvstr = pos.Mhst.Head.PGN
            gm <- gm 
                    + (if visw then ""
                        else (pos.Mhst.Length / 2 + 1).ToString() + ". ") 
                    + mvstr + " "
//            if mvstr.EndsWith("#") then 
//                ghdr <- { ghdr with Result = if visw then Bwin else Wwin }
            (gm, ghdr) |> gmchngEvt.Trigger
            if pos.Mhst.Head.PGN.EndsWith("#") then 
                fendEvt.Trigger()
                false
            else 
                let uopn = opts.Guseopn
                if uopn then 
                    let curv = vstt.CurVarn
                    let mvl = Varn.findnmvs pos curv.Brchs
                    if mvl.Length > 0 then 
                        let pos = pstt.CurPos
                        pstt.SetPos(Posn.DoMove(mvl.Head, pos))
                        let pos = pstt.CurPos
                        gm <- gm 
                                + (if not visw then ""
                                    else (pos.Mhst.Length / 2 + 1).ToString() + ". ") 
                                + pos.Mhst.Head.PGN + " "
                        (gm, ghdr) |> gmchngEvt.Trigger
                        //remove promotion as FICS defaults to Q unless you send "promote n"
                        //see http://www.freechess.org/Help/HelpFiles/promote.html
                        x.FicsSend(mvl.Head.UCI.Substring(0, 4))
                true
        else true
    member x.SeekFics() = 
        let mode = sst.Mode
        match mode with
        | DoFics -> 
            let opts = Opts.load()
            
            let rec processBuffer(buffer : string) = 
                let buf = buffer.Trim([| char ("\000") |]).Trim()
                if buf.Contains("Creating: pjbbwfc") then x.DoFicsW(opts,buf)
                elif buf.Contains("Creating: ") then x.DoFicsB(opts,buf)
                elif buf.Contains("{Game") then x.DoFicsEnd(buf)
                elif buf.Contains("<12>") then x.DoFicsMove(opts,buf)
                else true
            
            and onReceiveData(ar : IAsyncResult) = 
                _socket <- ar.AsyncState :?> Socket
                if (_socket <> null && _socket.Connected) then 
                    try 
                        let bytesReceived = _socket.EndReceive(ar)
                        if bytesReceived > 0 then 
                            let buffer = Encoding.ASCII.GetString(_inBuffer, 0, 1024)
                            buffer |> fmsgEvt.Trigger
                            let cont = processBuffer(buffer)
                            _inBuffer <- null
                            if cont then setupReceiveCallback(_socket)
                        else setupReceiveCallback(_socket)
                    with
                    | :? SocketException as ex -> 
                        MessageBox.Show("Error Receiving Data: " + ex.ToString())|> ignore
                        if _socket <> null then _socket.Close()
                    | ex -> 
                        MessageBox.Show(ex.Message, "Error processing receive buffer!")|> ignore
            
            and setupReceiveCallback(sock : Socket) = 
                try 
                    let receiveData = new AsyncCallback(onReceiveData)
                    _inBuffer <- Array.zeroCreate 1024
                    sock.BeginReceive(_inBuffer, 0, 1024, SocketFlags.None, receiveData, sock)|> ignore
                with ex -> 
                    MessageBox.Show("Setup receive callback failed: " + ex.ToString())|> ignore
            
            try 
                if _socket.Connected then 
                    x.FicsSend("seek " + opts.Ftime.ToString())
                    setupReceiveCallback(_socket)
                else 
                    MessageBox.Show("Unable to connect to remote host.")|> ignore
            with ex -> MessageBox.Show(ex.Message, "Connection Error") |> ignore
        | _ -> ()
    member x.StartFics() = 
        let doseek = ref true
        sst.SetMode(DoFics)
        let opts = Opts.load()
        let rec processBuffer(buffer : string) = 
            let buf = buffer.Trim([| char ("\000") |]).Trim()
            if buf.EndsWith("login:") then 
                x.FicsSend(opts.Funame)
                true
            elif buf.EndsWith("password:") then 
                x.FicsSend(opts.Fpass)
                false
            else true
        and onReceiveData(ar : IAsyncResult) = 
            _socket <- ar.AsyncState :?> Socket
            if (_socket <> null && _socket.Connected) then 
                try 
                    let bytesReceived = _socket.EndReceive(ar)
                    if bytesReceived > 0 then 
                        let buffer = Encoding.ASCII.GetString(_inBuffer, 0, 1024)
                        buffer |> fmsgEvt.Trigger
                        let cont = processBuffer(buffer)
                        _inBuffer <- null
                        if cont then setupReceiveCallback(_socket)
                    else setupReceiveCallback(_socket)
                with
                | :? SocketException as ex -> 
                    MessageBox.Show("Error Receiving Data: " + ex.ToString())|> ignore
                    if _socket <> null then _socket.Close()
                | ex -> 
                    MessageBox.Show(ex.Message, "Error processing receive buffer!")|> ignore
        and setupReceiveCallback(sock : Socket) = 
            try 
                let receiveData = new AsyncCallback(onReceiveData)
                _inBuffer <- Array.zeroCreate 1024
                sock.BeginReceive(_inBuffer, 0, 1024, SocketFlags.None, receiveData, sock) 
                |> ignore
            with ex -> 
                MessageBox.Show("Setup receive callback failed: " + ex.ToString())|> ignore
        
        //OnConnect
        let onConnect(ar : IAsyncResult) = 
            _socket <- ar.AsyncState :?> Socket
            try 
                _socket.EndConnect(ar)
                if _socket.Connected then setupReceiveCallback(_socket)
                else MessageBox.Show("Unable to connect to remote host.") |> ignore
            with ex -> MessageBox.Show(ex.Message, "Connection Error") |> ignore
        _socket <- new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
        try 
            _socket.Blocking <- false
            let onconnect = new AsyncCallback(onConnect)
            _socket.BeginConnect("freechess.org", 5000, onconnect, _socket)|> ignore
        with
        | :? SocketException as ex -> 
            MessageBox.Show("Socket error " + ex.ErrorCode.ToString() + " on BeginConnect")|> ignore
        | ex -> 
            MessageBox.Show("Unable to initiate connection: " + ex.ToString())|> ignore
    member x.SavePGN() = 
        let pstt = sst.Pstt
        let pos = pstt.CurPos
        let mode = sst.Mode
        match mode with
//        | DoPlay -> Game.updPGN pos (ghdr : Gmhdr) "EngGames.pgn"
//        | DoFics -> Game.updPGN pos (ghdr : Gmhdr) "FicsGames.pgn"
        | _ -> ()
    member x.OpenPGN(nm) = 
        pgngms <- Posn.loadPGN nm
        sst.SetMode(DoDb)
        pgngms |> dbldEvt.Trigger
    member x.LoadPgnGame(rw) = 
        let pstt = sst.Pstt
        //TODO
//        if pgngms.Length > 0 then pstt.SetPos(Posn.pgn2pos pgngms.[rw - 1])
        let pos = pstt.CurPos
        gm <- Posn.psn2pgn pos
        (gm, pgngms.[rw - 1]) |> dbgmldEvt.Trigger
        pgnmvs <- pos.Mhst
                  |> List.rev
                  |> List.toArray
    member x.GetPgnPos(off) = 
        let pstt = sst.Pstt
        let pos = pstt.CurPos
        let cnum = pos.Mhst.Length - 1
        let p = 
            if off = 0 then Posn.st
            elif off = 1 then 
                Posn.DoMoves(pgnmvs.[0..(min (cnum + 1) (pgnmvs.Length - 1))]|> List.ofArray, Posn.st)
            elif off = -1 then 
                if cnum > 0 then 
                    Posn.DoMoves(pgnmvs.[0..cnum - 1] |> List.ofArray, Posn.st)
                else Posn.st
            else Posn.DoMoves(pgnmvs |> List.ofArray, Posn.st)
        pstt.SetPos(p)
    member x.TrigGmChng(gc) = gc|>gmchngEvt.Trigger

and SharedState() as x = 
    let mutable mode = DoVarn
    //Posn
    let pstt = new PosnState(x)
    //Varn
    let vstt = new VarnState(x)
    //Test
    let tstt = new TestState(x)
    //Anal
    let astt = new AnalState(x)
    //Game
    let gstt = new GameState(x)
    //Events
    let mchngEvt = new Event<_>()
    //publish
    member x.ModeChng = mchngEvt.Publish
    //Members
    member x.Pstt = pstt
    member x.Vstt = vstt
    member x.Tstt = tstt
    member x.Astt = astt
    member x.Gstt = gstt
    member x.Mode = mode
    member x.SetMode(md) =
        mode <- md
        mode |> mchngEvt.Trigger
    member x.DoMode() = 
        let pos = pstt.CurPos
        match mode with
        | DoVarn -> 
            //update Varn
            let curv = vstt.CurVarn
            vstt.SetVarn(Varn.add curv pos)
            let curv = vstt.CurVarn
            let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
            (curv |> Varn.lines, currAnls) |> vstt.TrigCurv
            //update selected cell
            let oselvar = Varn.findsv pos curv.Brchs
            if oselvar.IsSome then 
                vstt.SetVar(oselvar.Value)
                let selvar = vstt.SelVar
                let selmv = pos.Mhst.Length - 1
                (selvar, selmv) |> vstt.TrigSelv
        | DoTest -> 
            //update tests
            tstt.SetDone(tstt.Done + 1)
            Application.DoEvents()
            System.Threading.Thread.Sleep(1000)
            let cormvto = tstt.Tests.[tstt.Num].Mv.To
            if cormvto = pos.Mhst.Head.To then 
                tstt.SetTest(tstt.Num, { tstt.Tests.[tstt.Num] with Status = "Passed" })
                tstt.SetCor(tstt.Cor + 1)
            else tstt.SetTest(tstt.Num, { tstt.Tests.[tstt.Num] with Status = "Failed" })
            (tstt.Num, tstt.Tests.[tstt.Num].Status) |> tstt.TrigRes
        | DoPlay -> 
            //update game and start analysis
            let visw = vstt.VarIsw
            let mvstr = pos.Mhst.Head.PGN
            gstt.SetGm(gstt.Gm + (if visw then (pos.Mhst.Length / 2 + 1).ToString() + ". "
                        else "") + mvstr + " ")
//            if mvstr.EndsWith("#") then 
//                gstt.SetGhdr({ gstt.Ghdr with Result = if visw then Wwin else Bwin })
            (gstt.Gm, gstt.Ghdr) |> gstt.TrigGmChng
            gstt.AnlPos()
        | DoFics -> 
            //update game and send move
            let visw = vstt.VarIsw
            let mvstr = pos.Mhst.Head.PGN
            gstt.SetGm(gstt.Gm + (if visw then (pos.Mhst.Length / 2 + 1).ToString() + ". "
                        else "") + mvstr + " ")
//            if mvstr.EndsWith("#") then 
//                gstt.SetGhdr({ gstt.Ghdr with Result = if visw then Wwin else Bwin })
            (gstt.Gm, gstt.Ghdr) |> gstt.TrigGmChng
            //remove promotion as FICS defaults to Q unless you send "promote n"
            //see http://www.freechess.org/Help/HelpFiles/promote.html 
            gstt.FicsSend(pos.Mhst.Head.UCI.Substring(0, 4))
        | DoDb -> ()
    member x.GetOpts() = Opts.load()
    member x.SaveOpts(opts) = Opts.save (opts)

module State =
    let stt = new SharedState()
    let pstt = stt.Pstt
    let vstt = stt.Vstt
    let tstt = stt.Tstt
    let astt = stt.Astt
    let gstt = stt.Gstt