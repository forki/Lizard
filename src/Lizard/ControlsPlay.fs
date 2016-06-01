namespace LizardChessUI

open System.Drawing
open System.Windows.Forms
open WeifenLuo.WinFormsUI.Docking
open SourceGrid
open DevAge.Drawing.VisualElements
open Dialogs
open State
open LizardChess.Types

module ControlsPlay =
    type Nav = 
        | Home
        | Prev
        | Next
        | End
        | NextL
        | PrevL

    type FicsData() as this = 
        inherit DockContent(Icon = ico "ficsm.ico", Text = "FICS Data", 
                            CloseButtonVisible = false)
        let pnlt = new Panel(Dock = DockStyle.Top, Height = 30)
        let flbl = 
            new Label(Dock = DockStyle.Top, Text = "No Fics Data for Position", 
                      TextAlign = ContentAlignment.BottomLeft)
        let dg = 
            new Grid(Dock = DockStyle.Fill, 
                     BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        
        //set up analysis
        let setFdt (fd : Ficsdata) = 
            let setfdt() = 
                if fd.MvList.Length > 0 then 
                    flbl.Text <- fd.ECO + ":" + fd.ECOName + "  Games:" 
                                 + fd.NumGames.ToString() + "  FEN:" 
                                 + fd.FENlong
                    //load dg header
                    dg.Rows.Clear()
                    dg.Rows.Insert(0)
                    dg.ColumnsCount <- 5
                    let addhdr i txt = 
                        let columnHeader = new Cells.ColumnHeader(txt)
                        columnHeader.View <- viewColHeader
                        dg.[0, i] <- columnHeader
                    [ "Move"; "White Win"; "Draw"; "Black Win"; "Number" ] 
                    |> List.iteri addhdr
                    dg.Columns.[0].Width <- 70
                    dg.Columns.[1].Width <- 70
                    dg.Columns.[2].Width <- 70
                    dg.Columns.[3].Width <- 70
                    dg.Columns.[4].Width <- 70
                    //load rows
                    let addr i (m : Ficsmv) = 
                        dg.Rows.Insert(i + 1)
                        dg.[i + 1, 0] <- new SourceGrid.Cells.Cell(m.Fpgn, 
                                                                   
                                                                   typedefof<string>)
                        dg.[i + 1, 0].View <- viewCell2
                        dg.[i + 1, 1] <- new SourceGrid.Cells.Cell(m.Wpc.ToString
                                                                       ("0.00") 
                                                                   + "%", 
                                                                   
                                                                   typedefof<string>)
                        dg.[i + 1, 1].View <- viewCell2
                        dg.[i + 1, 2] <- new SourceGrid.Cells.Cell(m.Dpc.ToString
                                                                       ("0.00") 
                                                                   + "%", 
                                                                   
                                                                   typedefof<string>)
                        dg.[i + 1, 2].View <- viewCell2
                        dg.[i + 1, 3] <- new SourceGrid.Cells.Cell(m.Bpc.ToString
                                                                       ("0.00") 
                                                                   + "%", 
                                                                   
                                                                   typedefof<string>)
                        dg.[i + 1, 3].View <- viewCell2
                        dg.[i + 1, 4] <- new SourceGrid.Cells.Cell(m.Fnum.ToString
                                                                       (), 
                                                                   
                                                                   typedefof<string>)
                        dg.[i + 1, 4].View <- viewCell2
                    fd.MvList
                    |> Array.mapi addr
                    |> ignore
                else 
                    flbl.Text <- "No Fics Data for Position: " + fd.FENlong
                    dg.Rows.Clear()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> setfdt())) |> ignore
                with _ -> ()
            else setfdt()
        
        do 
            this.Controls.Add(dg)
            pnlt.Controls.Add(flbl)
            this.Controls.Add(pnlt)
            //events
            pstt.FdtChng |> Observable.add setFdt
    
    type LineAnal() as this = 
        inherit DockContent(Icon = ico "cog.ico", Text = "Line Analysis", 
                            CloseButtonVisible = false)
        let pnlt = new Panel(Dock = DockStyle.Top, Height = 30)
        let albl = 
            new Label(Dock = DockStyle.Top, Text = "Stopped", 
                      TextAlign = ContentAlignment.BottomLeft)
        let sbtn = 
            new System.Windows.Forms.Button(Text = "Stop", 
                                            Dock = DockStyle.Right)
        let dg = 
            new Grid(Dock = DockStyle.Fill, 
                     BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        
        //set up analysis
        let setAnl start = 
            let setanl() = 
                if start then 
                    dg.Rows.Clear()
                    dg.Rows.Insert(0)
                    dg.ColumnsCount <- 4
                    let addhdr i txt = 
                        let columnHeader = new Cells.ColumnHeader(txt)
                        columnHeader.View <- viewColHeader
                        dg.[0, i] <- columnHeader
                    [ "Time"; "Message"; "Line"; "Depth" ] |> List.iteri addhdr
                    dg.Columns.[0].Width <- 60
                    dg.Columns.[1].Width <- 700
                    dg.Columns.[2].Width <- 400
                    dg.Columns.[3].Width <- 40
                    this.Activate()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> setanl())) |> ignore
                with _ -> ()
            else setanl()
        
        //set header
        let setHdr msg = 
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> albl.Text <- msg)) 
                    |> ignore
                with _ -> ()
            else albl.Text <- msg
        
        //add Message
        let addMsg (msg, ln, dpth) = 
            let addmsg() = 
                dg.Rows.Insert(1)
                dg.[1, 0] <- new SourceGrid.Cells.Cell(System.DateTime.Now.ToString
                                                           ("T"), 
                                                       typedefof<string>)
                dg.[1, 0].View <- viewCell2
                dg.[1, 1] <- new SourceGrid.Cells.Cell(msg, typedefof<string>)
                dg.[1, 1].View <- viewCell2
                dg.[1, 2] <- new SourceGrid.Cells.Cell(ln, typedefof<string>)
                dg.[1, 2].View <- viewCell2
                dg.[1, 3] <- new SourceGrid.Cells.Cell(dpth.ToString(), 
                                                       typedefof<string>)
                dg.[1, 3].View <- viewCell2
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> addmsg())) |> ignore
                with _ -> ()
            else addmsg()
        
        do 
            this.Controls.Add(dg)
            pnlt.Controls.Add(albl)
            pnlt.Controls.Add(sbtn)
            this.Controls.Add(pnlt)
            //events
            astt.AnlChng |> Observable.add setAnl
            astt.AnlHeadChng |> Observable.add setHdr
            astt.AnlMsg |> Observable.add addMsg
            sbtn.Click.Add(fun _ -> astt.AnlStop())

    type PosAnal() as this = 
        inherit DockContent(Icon = ico "cog2.ico", Text = "Analyse Position", 
                            CloseButtonVisible = false)
        let pnlt = new Panel(Dock = DockStyle.Top, Height = 30)
        let albl = 
            new Label(Dock = DockStyle.Top, Text = "Stopped", 
                      TextAlign = ContentAlignment.BottomLeft)
        let sbtn = 
            new System.Windows.Forms.Button(Text = "Stop", 
                                            Dock = DockStyle.Right)
        let dg = 
            new Grid(Dock = DockStyle.Fill, 
                     BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        
        //set up analysis
        let setAnl start = 
            let setanl() = 
                if start then 
                    dg.Rows.Clear()
                    dg.Rows.Insert(0)
                    dg.ColumnsCount <- 2
                    let addhdr i txt = 
                        let columnHeader = new Cells.ColumnHeader(txt)
                        columnHeader.View <- viewColHeader
                        dg.[0, i] <- columnHeader
                    [ "Time"; "Message" ] |> List.iteri addhdr
                    dg.Columns.[0].Width <- 60
                    dg.Columns.[1].Width <- 1100
                    this.Activate()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> setanl())) |> ignore
                with _ -> ()
            else setanl()
        
        //set header
        let setHdr msg = 
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> albl.Text <- msg)) 
                    |> ignore
                with _ -> ()
            else albl.Text <- msg
        
        //add Message
        let addMsg (msg) = 
            let addmsg() = 
                dg.Rows.Insert(1)
                dg.[1, 0] <- new SourceGrid.Cells.Cell(System.DateTime.Now.ToString
                                                           ("T"), 
                                                       typedefof<string>)
                dg.[1, 0].View <- viewCell2
                dg.[1, 1] <- new SourceGrid.Cells.Cell(msg, typedefof<string>)
                dg.[1, 1].View <- viewCell2
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(fun () -> addmsg())) |> ignore
                with _ -> ()
            else addmsg()
        
        do 
            this.Controls.Add(dg)
            pnlt.Controls.Add(albl)
            pnlt.Controls.Add(sbtn)
            this.Controls.Add(pnlt)
            //events
            astt.AnlpChng |> Observable.add setAnl
            astt.AnlpHeadChng |> Observable.add setHdr
            astt.AnlpMsg |> Observable.add addMsg
            sbtn.Click.Add(fun _ -> astt.AnlpStop())
    
    type Game() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, 
                            Text = "Game")
        let pnl = new Panel(Dock = DockStyle.Top, Height = 200)
        let gmrt = new RichTextBox(Dock = DockStyle.Fill)
        let dg = 
            new Grid(Dock = DockStyle.Fill, 
                     BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        
        let updgm (gm, ghdr) = 
            let upd() = 
                dg.Rows.Clear()
                dg.Rows.Insert(0)
                dg.ColumnsCount <- 1
                let addhdr i txt = 
                    let columnHeader = new Cells.ColumnHeader(txt)
                    columnHeader.View <- viewColHeader
                    dg.[0, i] <- columnHeader
                [ "Message" ] |> List.iteri addhdr
                dg.Columns.[0].Width <- 1000
                let nl = System.Environment.NewLine
                ()
//                gmrt.Text <- "White: " + ghdr.White + " Black: " + ghdr.Black 
//                             + nl + gm + " " + ghdr.Result.ToString()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(upd)) |> ignore
                with _ -> ()
            else upd()
        
        let updga (msg) = 
            let addmsg() = 
                dg.Rows.Insert(1)
                dg.[1, 0] <- new SourceGrid.Cells.Cell(msg, typedefof<string>)
                dg.[1, 0].View <- viewCell2
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(addmsg)) |> ignore
                with _ -> ()
            else addmsg()
        
        do 
            this.Controls.Add(dg)
            pnl.Controls.Add(gmrt)
            this.Controls.Add(pnl)
            //events
            gstt.GameChng |> Observable.add updgm
            gstt.GameAnlChng |> Observable.add updga
    
    type Fics() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, 
                            Text = "FICS")
        let tmpnl = new Panel(Dock = DockStyle.Top, Height = 30)
        let tmfl = 
            new FlowLayoutPanel(FlowDirection = FlowDirection.LeftToRight, 
                                Height = 30, Width = 460)
        let wtm = 
            new Label(Text = "300", TextAlign = ContentAlignment.MiddleRight, 
                      Font = new Font("Arial", 12.0F), 
                      ForeColor = Color.DarkGreen)
        let btm = 
            new Label(Text = "300", TextAlign = ContentAlignment.MiddleRight, 
                      Font = new Font("Arial", 12.0F), 
                      ForeColor = Color.DarkGreen)
        let tmr = new Timer(Interval = 1000)
        let mutable tisw = true
        let pnl = new Panel(Dock = DockStyle.Top, Height = 200)
        let gmrt = new RichTextBox(Dock = DockStyle.Fill)
        let frt = 
            new RichTextBox(Dock = DockStyle.Fill, BackColor = Color.DarkGreen, 
                            ForeColor = Color.White)
        
        let updgm (gm, ghdr) = 
            let upd() = 
                let nl = System.Environment.NewLine
                ()
//                gmrt.Text <- "White: " + ghdr.White + " Black: " + ghdr.Black 
//                             + nl + gm + " " + ghdr.Result.ToString()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(upd)) |> ignore
                with _ -> ()
            else upd()
        
        let updfm (msg) = 
            let addmsg() = 
                frt.AppendText(msg)
                frt.ScrollToCaret()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(addmsg)) |> ignore
                with _ -> ()
            else addmsg()
        
        let updtm (w, b, isw) = 
            let addtm() = 
                wtm.Text <- w.ToString()
                btm.Text <- b.ToString()
                tisw <- isw
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(addtm)) |> ignore
                with _ -> ()
            else addtm()
        
        let updtmr (e) = 
            let upd() = 
                if tisw then 
                    let ctm = int (wtm.Text)
                    wtm.Text <- (ctm - 1).ToString()
                else 
                    let ctm = int (btm.Text)
                    btm.Text <- (ctm - 1).ToString()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(upd)) |> ignore
                with _ -> ()
            else upd()
        
        let gmst() = 
            let st() = 
                tisw <- true
                wtm.Text <- "300"
                btm.Text <- "300"
                tmr.Start()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(st)) |> ignore
                with _ -> ()
            else st()
        
        let gmend() = 
            let en() = 
                tisw <- true
                wtm.Text <- "300"
                btm.Text <- "300"
                tmr.Stop()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(en)) |> ignore
                with _ -> ()
            else en()
        
        do 
            this.Controls.Add(frt)
            pnl.Controls.Add(gmrt)
            this.Controls.Add(pnl)
            tmfl.Controls.AddRange([| new Label(Text = "White:", 
                                                
                                                TextAlign = ContentAlignment.MiddleRight, 
                                                Font = new Font("Arial", 12.0F))
                                      wtm
                                      
                                      new Label(Text = "Black:", 
                                                
                                                TextAlign = ContentAlignment.MiddleRight, 
                                                Font = new Font("Arial", 12.0F))
                                      btm |])
            tmpnl.Controls.Add(tmfl)
            this.Controls.Add(tmpnl)
            frt.Clear()
            gmrt.Clear()
            //events
            tmr.Tick.Add(updtmr)
            tmr.Stop()
            gstt.GameChng |> Observable.add updgm
            gstt.FicsMsg |> Observable.add updfm
            gstt.FicsTim |> Observable.add updtm
            gstt.FicsStart |> Observable.add gmst
            gstt.FicsEnd |> Observable.add gmend
    
    type Db() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, 
                            Text = "Database")
        let pnl1 = new Panel(Dock = DockStyle.Top, Height = 30)
        let homeb = new ToolStripButton(Image = img "homeButton.png")
        let prevb = new ToolStripButton(Image = img "prevButton.png")
        let nextb = new ToolStripButton(Image = img "nextButton.png")
        let endb = new ToolStripButton(Image = img "endButton.png")
        let sep = new ToolStripSeparator()
        let prevl = new ToolStripButton(Image = img "prevLineButton.png")
        let nextl = new ToolStripButton(Image = img "nextLineButton.png")
        let ts = 
            new ToolStrip(Anchor = AnchorStyles.Left, 
                          GripStyle = ToolStripGripStyle.Hidden, 
                          Dock = DockStyle.None, Left = 10)
        let pnl2 = new Panel(Dock = DockStyle.Top, Height = 200)
        let gmrt = new RichTextBox(Dock = DockStyle.Fill)
        let dg = 
            new Grid(Dock = DockStyle.Fill, 
                     BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        
        // do navigation
        let donav (n) = 
            let p = dg.Selection.ActivePosition
            match n with
            | Home -> gstt.GetPgnPos(0)
            | Prev -> gstt.GetPgnPos(-1)
            | Next -> gstt.GetPgnPos(1)
            | End -> gstt.GetPgnPos(999)
            | PrevL -> 
                if p.Row > 1 then 
                    let np = new Position(p.Row - 1, p.Column)
                    dg.Selection.Focus(np, true) |> ignore
            | NextL -> 
                if p.Row < dg.RowsCount - 1 then 
                    let np = new Position(p.Row + 1, p.Column)
                    dg.Selection.Focus(np, true) |> ignore
        
        // keyDown
        let dokeydown (e : KeyEventArgs) = 
            e.Handled <- true
            let s = new obj()
            if (e.KeyCode = Keys.Home) then donav (Home)
            if (e.KeyCode = Keys.Up) then donav (Prev)
            if (e.KeyCode = Keys.Down) then donav (Next)
            if (e.KeyCode = Keys.End) then donav (End)
            if (e.KeyCode = Keys.Left) then donav (PrevL)
            if (e.KeyCode = Keys.Right) then donav (NextL)
        
        let upddg (gms : Lizard.PGN.Game list) = 
            dg.Rows.Clear()
            dg.Rows.Insert(0)
            dg.ColumnsCount <- 8
            let addhdr i txt = 
                let columnHeader = new Cells.ColumnHeader(txt)
                columnHeader.View <- viewColHeader
                dg.[0, i + 1] <- columnHeader
            dg.[0, 0] <- new Cells.ColumnHeader("")
            [ "White"; "Black"; "Event"; "Site"; "Round"; "Date"; "Result" ] 
            |> List.iteri addhdr
            let addcell r c cl = 
                dg.[r, c + 1] <- new SourceGrid.Cells.Cell(cl, typedefof<string>)
                dg.[r, c + 1].View <- if (c + 1) % 2 = 0 then viewCell1
                                      else viewCell2
            
            let addr r (gm : Lizard.PGN.Game) = 
                dg.Rows.Insert(r + 1)
                let rowheader = 
                    new SourceGrid.Cells.Cell(r + 1, typedefof<string>)
                rowheader.View <- viewRowHeader
                dg.[r + 1, 0] <- rowheader
                gm.White |> addcell (r + 1) 0
                gm.Black |> addcell (r + 1) 1
                gm.Event |> addcell (r + 1) 2
                gm.Site |> addcell (r + 1) 3
                gm.Round |> addcell (r + 1) 4
                gm.DateStr |> addcell (r + 1) 5
                gm.Result.ToString() |> addcell (r + 1) 6
            
            gms |> List.iteri addr
            dg.AutoSizeCells()
            if dg.RowsCount > 1 then 
                let np = new Position(1, 1)
                dg.Selection.Focus(np, true) |> ignore
        
        let updgm (gm, pgngm:Lizard.PGN.Game) = 
            let upd() = 
                let nl = System.Environment.NewLine
                gmrt.Text <- "White: " + pgngm.White + " Black: " 
                             + pgngm.Black + nl + gm + " " 
                             + pgngm.Result.ToString()
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(upd)) |> ignore
                with _ -> ()
            else upd()
        
        // selgm called when cell is selected
        let selgm (e : RangeRegionChangedEventArgs) = 
            if e.AddedRange <> null && e.AddedRange.Count = 1 
               && dg.RowsCount > 1 then 
                let cl = e.AddedRange.GetCellsPositions().[0]
                if cl.Row > 0 && cl.Column > 0 then gstt.LoadPgnGame(cl.Row)
        
        do 
            this.Controls.Add(dg)
            pnl2.Controls.Add(gmrt)
            this.Controls.Add(pnl2)
            [ homeb; prevb; nextb; endb ] 
            |> List.iter (fun c -> ts.Items.Add(c) |> ignore)
            ts.Items.Add(sep) |> ignore
            [ prevl; nextl ] |> List.iter (fun c -> ts.Items.Add(c) |> ignore)
            pnl1.Controls.Add(ts)
            this.Controls.Add(pnl1)
            //events
            dg.Selection.SelectionChanged.Add(selgm)
            //TODO
//            gstt.DbGameLoad |> Observable.add updgm
//            gstt.DbLoad |> Observable.add upddg
            homeb.Click.Add(fun _ -> donav (Home))
            prevb.Click.Add(fun _ -> donav (Prev))
            nextb.Click.Add(fun _ -> donav (Next))
            endb.Click.Add(fun _ -> donav (End))
            prevl.Click.Add(fun _ -> donav (PrevL))
            nextl.Click.Add(fun _ -> donav (NextL))
            dg.KeyDown.Add(dokeydown)