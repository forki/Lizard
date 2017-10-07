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
    //Events
    let pchngEvt = new Event<_>()
    let mchngEvt = new Event<_>()
    let promEvt = new Event<_>()
    let pssqsEvt = new Event<_>()
    let orntEvt = new Event<_>()
    //publish
    member x.PosChng = pchngEvt.Publish
    member x.MvsChng = mchngEvt.Publish
    member x.Prom = promEvt.Publish
    member x.PsSqsChng = pssqsEvt.Publish
    member x.Ornt = orntEvt.Publish
    
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
            |> List.map (fun (m : Move1) -> m.UCI)
            |> List.reduce (fun a b -> a + " " + b)
    
    member x.PsSqs 
        with get () = pssqs
        and set (value) = 
            pssqs <- value
            pssqs |> pssqsEvt.Trigger
    
    member x.Move(mfrom, mto) = 
        if pssqs.Length > 0 then 
            let mv = pos.GetMvFT(mfrom, mto)
            if mv.Mtyp = Prom('Q') then (mv, pos.IsW) |> promEvt.Trigger
            elif mv.Mtyp <> Invalid then 
                x.Pos.DoMv mv
                x.Pos <- x.Pos
                x.Mvs <- x.Mvs @ [ mv ]
                x.PsSqs <- []
                sst.DoMode()
            else 
                pos |> pchngEvt.Trigger
                x.PsSqs <- []
    
    member x.Promote(mv) = 
        x.Pos.DoMv mv
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
    
    member x.GetPos(vr, mv) = 
        let mvl = Varn.mvl (curv, vr, mv)
        selvar <- vr
        let pstt : PosnState = sst.Pstt
        pstt.Mvs <- mvl
        pstt.Pos <- Pos.FromMoves(mvl)
    
    member x.OpenVarn(nm, isw) = 
        let pstt : PosnState = sst.Pstt
        pstt.Pos <- Pos.Start()
        pstt.Mvs <- []
        sst.SetMode(DoVarn)
        visw <- isw
        curv <- Varn.load (nm, visw)
        curv |> cchngEvt.Trigger
        visw |> pstt.TrigOri
    
    member x.SaveVarn() = Varn.save (curv)
    
    member x.NewVarn(nm, isw) = 
        let pstt : PosnState = sst.Pstt
        pstt.Pos <- Pos.Start()
        pstt.Mvs <- []
        sst.SetMode(DoVarn)
        curv <- Varn.cur (nm, isw)
        curv |> cchngEvt.Trigger
        visw |> pstt.TrigOri
        if isw then wvrs <- nm :: wvrs
        else bvrs <- nm :: bvrs
        visw <- isw
        visw |> vchngEvt.Trigger
    
    member x.SaveAsVarn(nm) = 
        curv <- Varn.saveas (curv, nm)
        curv |> cchngEvt.Trigger
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
        curv |> cchngEvt.Trigger
    
    member x.GetNextMvs() = 
        let pstt : PosnState = sst.Pstt
        Varn.findnmvs pstt.Mvs curv.Brchs
    
    member x.DoNextMv(mv) = 
        let pstt : PosnState = sst.Pstt
        let move = pstt.Pos.GetMv(mv)
        pstt.Pos.DoMv(move)
        pstt.Mvs <- pstt.Mvs @ [ move ]
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
    let apchngEvt = new Event<_>()
    let ahpchngEvt = new Event<_>()
    let apmsgEvt = new Event<_>()
    
    //publish
    member x.AnlpChng = apchngEvt.Publish
    member x.AnlpHeadChng = ahpchngEvt.Publish
    member x.AnlpMsg = apmsgEvt.Publish
    
    //members
    //Analyse Pos
    member x.AnlpStart() = 
        procp <- new System.Diagnostics.Process()
        let eng = "stockfish.exe"
        let send msg = Eng.Send(msg, procp)
        
        //p_out
        let pOut (e : System.Diagnostics.DataReceivedEventArgs) = 
            if not (e.Data = null || e.Data = "") then 
                let msg = e.Data.ToString().Trim()
                if not (msg.StartsWith("info") && not (msg.Contains(" cp "))) then msg |> apmsgEvt.Trigger
        procp.OutputDataReceived.Add(pOut)
        //Start process
        Eng.SetUpPrc procp eng
        // call calcs
        // need to send game position moves as UCI
        let pstt = sst.Pstt
        Eng.ComputeAnswer(pstt.MvsStr, 99, procp)
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
            curv |> vstt.TrigCurv
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
    
module State = 
    let stt = new SharedState()
    let pstt = stt.Pstt
    let vstt = stt.Vstt
    let tstt = stt.Tstt
    let astt = stt.Astt
