namespace LizardChessUI

open Lizard
open System
open System.IO
open System.Net.Sockets
open System.Threading
open System.Windows.Forms
open System.Text

type Mode = 
    | DoVarn
    | DoTest

type PosnState(sst : SharedState) = 
    let mutable pos = Pos.Start()
    let mutable mvs = []
    let mutable pssqs : int list = []
    let mutable cfdt = FcsDt.empfdb
    let mutable canl = Eng.empanl
    //Events
    let pchngEvt = new Event<_>()
    let mchngEvt = new Event<_>()
    let bmchngEvt = new Event<_>()
    let promEvt = new Event<_>()
    let pssqsEvt = new Event<_>()
    let orntEvt = new Event<_>()
    let fdtchngEvt = new Event<_>()
    //publish
    member x.PosChng = pchngEvt.Publish
    member x.MvsChng = mchngEvt.Publish
    member x.BmChng = bmchngEvt.Publish
    member x.Prom = promEvt.Publish
    member x.PsSqsChng = pssqsEvt.Publish
    member x.Ornt = orntEvt.Publish
    member x.FdtChng = fdtchngEvt.Publish
    
    //members
    member x.Pos 
        with get () = pos
        and set (value) = 
            pos <- value
            pos |> pchngEvt.Trigger
    
    member x.Mvs 
        with get () = mvs
        and set (value) = 
            mvs <- value
            mvs |> mchngEvt.Trigger
    
    member x.MvsStr = 
        if List.isEmpty x.Mvs then ""
        else 
            x.Mvs
            |> List.map (fun (m : Move) -> m.UCI)
            |> List.reduce (fun a b -> a + " " + b)
    
    member x.PsSqs 
        with get () = pssqs
        and set (value) = 
            pssqs <- value
            pssqs |> pssqsEvt.Trigger
    
    member x.SetCanl() = 
        canl <- x.MvsStr |> Eng.getanl (Eng.loadLineStore())
        canl |> bmchngEvt.Trigger
    
    member x.SetCfdt() = 
        cfdt <- x.MvsStr |> FcsDt.getfdb (FcsDt.loadFicsDbStore())
        cfdt |> fdtchngEvt.Trigger
    
    member x.Move(mfrom, mto) = 
        if pssqs.Length > 0 then 
            let mv = pos.GetMvFT(mfrom, mto)
            if mv.Mtyp = Prom('Q') then (mv, pos.IsW) |> promEvt.Trigger
            elif mv.Mtyp <> Invalid then 
                x.Pos.DoMv mv
                x.Pos <- x.Pos
                x.Mvs <- x.Mvs @ [ mv ]
                x.SetCanl()
                x.SetCfdt()
                x.PsSqs <- []
                sst.DoMode()
            else 
                pos |> pchngEvt.Trigger
                x.SetCanl()
                x.SetCfdt()
                x.PsSqs <- []
    
    member x.Promote(mv) = 
        x.Pos.DoMv mv
        x.SetCanl()
        x.SetCfdt()
        sst.DoMode()
    
    member x.GetPossSqs(mfrom) = x.PsSqs <- pos.GetPossSqs(mfrom)
    member x.TrigOri(isw) = isw |> orntEvt.Trigger

and VarnState(sst : SharedState) = 
    let mutable wvrs = Varn.wvars()
    let mutable bvrs = Varn.bvars()
    let mutable visw = true
    
    let mutable curv = 
        if visw then 
            if wvrs.Length > 1 then Varn.load (wvrs.[0], visw)
            else Varn.emp
        else if bvrs.Length > 1 then Varn.load (bvrs.[0], visw)
        else Varn.emp
    
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
        let pstt : PosnState = sst.Pstt
        pstt.Mvs <- mvl
        pstt.Pos <- Pos.FromMoves(mvl)
        pstt.SetCanl()
        pstt.SetCfdt()
    
    member x.OpenVarn(nm, isw) = 
        let pstt : PosnState = sst.Pstt
        pstt.Pos <- Pos.Start()
        pstt.Mvs <- []
        sst.SetMode(DoVarn)
        visw <- isw
        curv <- Varn.load (nm, visw)
        let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
        (curv |> Varn.lines, currAnls) |> cchngEvt.Trigger
        visw |> pstt.TrigOri
    
    member x.SaveVarn() = Varn.save (curv)
    
    member x.NewVarn(nm, isw) = 
        let pstt : PosnState = sst.Pstt
        pstt.Pos <- Pos.Start()
        pstt.Mvs <- []
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
        let pstt : PosnState = sst.Pstt
        Varn.findnmvs pstt.Mvs curv.Brchs
    
    member x.DoNextMv(mv) = 
        let pstt : PosnState = sst.Pstt
        let move = pstt.Pos.GetMv(mv)
        pstt.Pos.DoMv(move)
        pstt.Mvs <- pstt.Mvs @ [ move ]
        pstt.SetCanl()
        pstt.SetCfdt()
        let oselvar = Varn.findsv pstt.Mvs curv.Brchs
        if oselvar.IsSome then 
            selvar <- oselvar.Value
            let selmv = pstt.Mvs.Length - 1
            (selvar, selmv) |> selCelEvt.Trigger
    
    member x.TrigCurv(cc) = cc |> cchngEvt.Trigger
    member x.TrigSelv(ss) = ss |> selCelEvt.Trigger

and TestState(sst : SharedState) = 
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
    member x.SetTest(i, t) = tests.[i] <- t
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
        let pstt : PosnState = sst.Pstt
        if numtst <> i then 
            pstt.Pos <- Test.GetPosn(tests.[i])
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
    
    member x.TrigRes(rs) = rs |> tresEvt.Trigger

and AnalState(sst : SharedState) = 
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
    let rec getAnswer (processBM, answer) linestore (vn : string []) (opts, eng, nm) = 
        let send msg = Game.Send(msg, proc)
        if (processBM) then Eng.procbm (linestore, ln, mvct, answer, [ lastmsg; lastmsg1; lastmsg2 ])
        Eng.saveLineStore (linestore)
        let lnctnew = 
            if mvct = ln.Trim().Split(' ').Length then lnct + 1
            else -1
        if lnctnew = vn.Length then 
            if (dpth = opts.Emaxdepth) then 
                send ("stop")
                send ("")
                let text = "Finished processing variation " + nm + " to depth " + dpth.ToString()
                text |> ahchngEvt.Trigger
            else 
                ln <- vn.[0].Trim()
                lnct <- 0
                let mvs = ln.Split(' ')
                mvct <- min (mvs.Length - 1) 5
                dpth <- dpth + 1
                //clear for next line
                Eng.hdr (eng, ln, lnct, mvct, dpth) |> ahchngEvt.Trigger
                if (Eng.alreadyDone (linestore, Eng.str2str (ln, mvct), dpth)) then 
                    getAnswer (false, "") linestore vn (opts, eng, nm)
                else Game.ComputeAnswer(Eng.str2str (ln, mvct), dpth, proc)
        else 
            if lnctnew <> -1 then 
                let mvs = vn.[lnctnew].Trim().Split(' ')
                mvct <- min (mvs.Length - 1) 5
                ln <- vn.[lnctnew].Trim()
                lnct <- lnctnew
            else mvct <- mvct + 1
            //clear for next line
            Eng.hdr (eng, ln, lnct, mvct, dpth) |> ahchngEvt.Trigger
            //do next one
            if (Eng.alreadyDone (linestore, Eng.str2str (ln, mvct), dpth)) then 
                getAnswer (false, "") linestore vn (opts, eng, nm)
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
        let vstt : VarnState = sst.Vstt
        let curv = vstt.CurVarn
        let strs = Varn.cur2txt (curv)
        let vnpsns = curv.Brchs
        Seq.map2 (Eng.strmvl2bms (Eng.loadLineStore())) strs vnpsns |> Seq.toArray
    
    //Analyse Line
    member x.AnlStart(nm, isw) = 
        proc <- new System.Diagnostics.Process()
        let vn = Varn.loadtxta (nm, isw)
        let linestore = Eng.loadLineStore()
        let opts = Opts.load()
        let eng = "stockfish.exe"
        
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
                    if token.Length > 1 then getAnswer (true, token.[1]) linestore vn (opts, eng, nm)
        proc.OutputDataReceived.Add(pOut)
        //Start process
        Game.SetUpPrc proc eng
        //set mutables
        ln <- vn.[0].Trim()
        let mvs = ln.Split(' ')
        lnct <- 0
        mvct <- min (mvs.Length - 1) 5
        dpth <- 10
        // call calcs
        if (Eng.alreadyDone (linestore, Eng.str2str (ln, mvct), dpth)) then 
            getAnswer (false, "") linestore vn (opts, eng, nm)
        else Game.ComputeAnswer(Eng.str2str (ln, mvct), dpth, proc)
        isanl <- true
        isanl |> achngEvt.Trigger
        Eng.hdr (eng, ln, lnct, mvct, dpth) |> ahchngEvt.Trigger
    
    member x.AnlStop() = 
        if proc <> null then proc.Kill()
        isanl <- false
        isanl |> achngEvt.Trigger
        "Stopped" |> ahchngEvt.Trigger
    
    //Analyse Pos
    member x.AnlpStart() = 
        procp <- new System.Diagnostics.Process()
        let opts = Opts.load()
        let eng = "stockfish.exe"
        let send msg = Game.Send(msg, procp)
        
        //p_out
        let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
            if not (e.Data = null || e.Data = "") then 
                let msg = e.Data.ToString().Trim()
                if not (msg.StartsWith("info") && not (msg.Contains(" cp "))) then msg |> apmsgEvt.Trigger
        procp.OutputDataReceived.Add(pOut)
        //Start process
        Game.SetUpPrc procp eng
        // call calcs
        // need to send game position moves as UCI
        let pstt = sst.Pstt
        Game.ComputeAnswer(pstt.MvsStr, 99, procp)
        isanl <- true
        isanl |> apchngEvt.Trigger
        (eng + " - " + (pstt.MvsStr)) |> ahpchngEvt.Trigger
    
    member x.AnlpStop() = 
        if procp <> null then procp.Kill()
        isanl <- false
        isanl |> apchngEvt.Trigger
        "Stopped" |> ahpchngEvt.Trigger

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
    //Events
    let mchngEvt = new Event<_>()
    //publish
    member x.ModeChng = mchngEvt.Publish
    //Members
    member x.Pstt = pstt
    member x.Vstt = vstt
    member x.Tstt = tstt
    member x.Astt = astt
    member x.Mode = mode
    
    member x.SetMode(md) = 
        mode <- md
        mode |> mchngEvt.Trigger
    
    member x.DoMode() = 
        match mode with
        | DoVarn -> 
            //update Varn
            let curv = vstt.CurVarn
            vstt.SetVarn(Varn.add curv pstt.Mvs)
            let curv = vstt.CurVarn
            let currAnls = Eng.getanls (Eng.loadLineStore(), curv)
            (curv |> Varn.lines, currAnls) |> vstt.TrigCurv
            //update selected cell
            let oselvar = Varn.findsv pstt.Mvs curv.Brchs
            if oselvar.IsSome then 
                vstt.SetVar(oselvar.Value)
                let selvar = vstt.SelVar
                let selmv = pstt.Mvs.Length - 1
                (selvar, selmv) |> vstt.TrigSelv
        | DoTest -> 
            //update tests
            tstt.SetDone(tstt.Done + 1)
            Application.DoEvents()
            System.Threading.Thread.Sleep(1000)
            let cormvto = tstt.Tests.[tstt.Num].Mv.Mto
            if cormvto = pstt.Mvs.[pstt.Mvs.Length - 1].Mto then 
                tstt.SetTest(tstt.Num, { tstt.Tests.[tstt.Num] with Status = "Passed" })
                tstt.SetCor(tstt.Cor + 1)
            else tstt.SetTest(tstt.Num, { tstt.Tests.[tstt.Num] with Status = "Failed" })
            (tstt.Num, tstt.Tests.[tstt.Num].Status) |> tstt.TrigRes
    
    member x.GetOpts() = Opts.load()
    member x.SaveOpts(opts) = Opts.save (opts)

module State = 
    let stt = new SharedState()
    let pstt = stt.Pstt
    let vstt = stt.Vstt
    let tstt = stt.Tstt
    let astt = stt.Astt
