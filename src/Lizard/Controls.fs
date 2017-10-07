namespace LizardChessUI

open System.Drawing
open System.Windows.Forms
open WeifenLuo.WinFormsUI.Docking
open SourceGrid
open DevAge.Drawing.VisualElements
open Dialogs
open State
open Lizard.Types
open Lizard

module Controls = 
    type TsRib() as this = 
        inherit Ribbon(Height = 140, OrbStyle = RibbonOrbStyle.Office_2010, OrbVisible = false)
        let sv = new RibbonButton("New", SmallImage = img "save16.png")
        //Openings
        let otab = new RibbonTab("Openings")
        //Variation
        let vrpl = new RibbonPanel("Variation")
        let nw = new RibbonButton("New", Image = img "new.png")
        let opn = new RibbonButton("Open", Image = img "opn.png")
        let wbtn = new RibbonButton("White", Style = RibbonButtonStyle.DropDownListItem)
        let bbtn = new RibbonButton("Black", Style = RibbonButtonStyle.DropDownListItem)
        let wb = new RibbonComboBox(TextBoxWidth = 150)
        let vl = new RibbonComboBox(TextBoxWidth = 150)
        let sav = new RibbonButton("Save", Image = img "sav.png")
        let sava = new RibbonButton("Save As", Image = img "sava.png")
        let del = new RibbonButton("Delete", Image = img "del.png")
        let delline = new RibbonButton("Delete Line", Image = img "delline.png")
        //Test
        let trpl = new RibbonPanel("Test")
        let rt = new RibbonButton("Run", Image = img "Train.png", Style = RibbonButtonStyle.SplitDropDown)
        let rrt = new RibbonButton("Run Random Test", SmallImage = img "rantest.png")
        let rlt = new RibbonButton("Run Linear Test", SmallImage = img "lintest.png")
        let tr = new RibbonButton("Results", Image = img "rosette.png", Style = RibbonButtonStyle.SplitDropDown)
        let rtr = new RibbonButton("Random Results", SmallImage = img "ranres.png")
        let ltr = new RibbonButton("Linear Results", SmallImage = img "linres.png")
        //Analyse
        let arpl = new RibbonPanel("Analyse")
        let anll = new RibbonButton("Line", Image = img "cog.png")
        let anlp = new RibbonButton("Position", Image = img "cog2.png")
        //Support
        let srpl = new RibbonPanel("Support")
        let opt = new RibbonButton("Options", Image = img "options.png")
        let src = new RibbonButton("Source Code", MaxSizeMode = RibbonElementSizeMode.Medium, SmallImage = null)
        let cns = new RibbonButton("Conservation", MaxSizeMode = RibbonElementSizeMode.Medium, SmallImage = null)
        
        // save variation
        let dosave (e) = 
            let msg = vstt.SaveVarn()
            MessageBox.Show(msg) |> ignore
        
        // delete variation
        let dodel (e) = 
            let nm, isw = vl.SelectedItem.Text, wb.SelectedItem.Text = "White"
            if MessageBox.Show
                   ("Do you want to delete variation " + nm + "?", "Delete Variation", MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question) = DialogResult.Yes then 
                vstt.DelVarn(nm, isw)
                MessageBox.Show
                    ("Variation " + nm + " deleted!", "Delete Variation", MessageBoxButtons.OK, 
                     MessageBoxIcon.Exclamation) |> ignore
        
        // delete line
        let dodelline (e) = 
            let selvar = vstt.SelVar
            if selvar <> -1 
               && (MessageBox.Show
                       ("Do you want to delete line " + (selvar + 1).ToString() + " ?", "Delete Line", 
                        MessageBoxButtons.YesNo) = DialogResult.Yes) then vstt.DelLine()
        
        //load vrs for dropdown
        let loadvrs (isw) = 
            let vrs = vstt.Vars(isw)
            if isw then wb.SelectedItem <- wb.DropDownItems.[0]
            else wb.SelectedItem <- wb.DropDownItems.[1]
            vl.DropDownItems.Clear()
            vrs 
            |> List.iter 
                   (fun i -> vl.DropDownItems.Add(new RibbonButton(i, Style = RibbonButtonStyle.DropDownListItem)))
            if vl.DropDownItems.Count > 0 then vl.SelectedItem <- vl.DropDownItems.[0]
        
        //run random test
        let dorantest (e) = 
            let nm, isw = vl.SelectedItem.Text, wb.SelectedItem.Text = "White"
            tstt.LoadTest(true, nm, isw)
        
        //run linear test
        let dolintest (e) = 
            let nm, isw = vl.SelectedItem.Text, wb.SelectedItem.Text = "White"
            tstt.LoadTest(false, nm, isw)
        
        //show random results
        let showranres (e) = tstt.ShowRes(true)
        //show linear results
        let showlinres (e) = tstt.ShowRes(false)
        
        //analyse line
        let anlline (e) = 
            let nm, isw = vl.SelectedItem.Text, wb.SelectedItem.Text = "White"
            astt.AnlStart(nm, isw)
        
        //analyse pos
        let anlpos (e) = astt.AnlpStart()
        
        let addOpen() = 
            wb.DropDownItems.Add(wbtn)
            wb.DropDownItems.Add(bbtn)
            wb.SelectedItem <- wb.DropDownItems.[0]
            loadvrs (true)
            vrpl.Items.AddRange([ nw; opn; wb; vl; sav; sava; del; delline ])
            otab.Panels.Add(vrpl)
            rt.DropDownItems.AddRange([ rrt; rlt ])
            tr.DropDownItems.AddRange([ rtr; ltr ])
            trpl.Items.AddRange([ rt; tr ])
            otab.Panels.Add(trpl)
            arpl.Items.AddRange([ anll; anlp ])
            otab.Panels.Add(arpl)
            srpl.Items.AddRange([ opt; src; cns ])
            otab.Panels.Add(srpl)
            this.Tabs.Add(otab)
        
        do 
            this.QuickAcessToolbar.Items.Add(sv)
            //Openings
            addOpen()
            //events
            nw.Click.Add(fun _ -> (new DlgNew()).ShowDialog() |> ignore)
            opn.Click.Add(fun _ -> vstt.OpenVarn(vl.SelectedItem.Text, wb.SelectedItem.Text = "White"))
            sav.Click.Add(dosave)
            sava.Click.Add(fun _ -> (new DlgSaveAs()).ShowDialog() |> ignore)
            del.Click.Add(dodel)
            delline.Click.Add(dodelline)
            wbtn.Click.Add(fun _ -> loadvrs (true))
            bbtn.Click.Add(fun _ -> loadvrs (false))
            vstt.VarsChng |> Observable.add loadvrs
            rrt.Click.Add(dorantest)
            rlt.Click.Add(dolintest)
            rtr.Click.Add(showranres)
            ltr.Click.Add(showlinres)
            anll.Click.Add(anlline)
            anlp.Click.Add(anlpos)
            opt.Click.Add(fun _ -> (new DlgOpts()).ShowDialog() |> ignore)
            src.Click.Add(fun _ -> System.Diagnostics.Process.Start("https://github.com/pbbwfc/Lizard") |> ignore)
            cns.Click.Add(fun _ -> System.Diagnostics.Process.Start("http://www.arc-trust.org/") |> ignore)
    
    type Board() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, Text = "Board")
        let mutable sqTo = -1
        let mutable cCur = Cursors.Default
        let sqpnl = new Panel(Width = 420, Height = 420, Left = 29, Top = 13)
        
        let edges = 
            [ new Panel(BackgroundImage = img "Back.jpg", Width = 342, Height = 8, Left = 24, Top = 6)
              new Panel(BackgroundImage = img "Back.jpg", Width = 8, Height = 350, Left = 24, Top = 8)
              new Panel(BackgroundImage = img "Back.jpg", Width = 8, Height = 350, Left = 366, Top = 6)
              new Panel(BackgroundImage = img "Back.jpg", Width = 342, Height = 8, Left = 32, Top = 350) ]
        
        let bmpnl = new Panel(Width = 420, Height = 80, Left = 29, Top = 450)
        let bmlbl = 
            new Label(Text = "Best Move:", Dock = DockStyle.Fill, ForeColor = Color.Green, 
                      Font = new Font(FontFamily.GenericSansSerif, 18.0f, FontStyle.Bold))
        let sqs : PictureBox [] = Array.zeroCreate 64
        let flbls : Label [] = Array.zeroCreate 8
        let rlbls : Label [] = Array.zeroCreate 8
        let btnpnl = new Panel(Width = 420, Height = 40, Left = 29, Top = 530)
        let pbtn = new System.Windows.Forms.Button(Text="Copy PGN",Dock=DockStyle.Left)
        let fbtn = new System.Windows.Forms.Button(Text="Copy \"FEN\"",Dock=DockStyle.Left)
        
        /// get cursor given char
        let getcur c = 
            match c with
            | 'P' -> cur "WhitePawn.cur"
            | 'B' -> cur "WhiteBishop.cur"
            | 'N' -> cur "WhiteKnight.cur"
            | 'R' -> cur "WhiteRook.cur"
            | 'K' -> cur "WhiteKing.cur"
            | 'Q' -> cur "WhiteQueen.cur"
            | 'p' -> cur "BlackPawn.cur"
            | 'b' -> cur "BlackBishop.cur"
            | 'n' -> cur "BlackKnight.cur"
            | 'r' -> cur "BlackRook.cur"
            | 'k' -> cur "BlackKing.cur"
            | 'q' -> cur "BlackQueen.cur"
            | _ -> failwith "invalid piece"
        
        /// get image given char
        let getim c = 
            match c with
            | 'P' -> img "WhitePawn.png"
            | 'B' -> img "WhiteBishop.png"
            | 'N' -> img "WhiteKnight.png"
            | 'R' -> img "WhiteRook.png"
            | 'K' -> img "WhiteKing.png"
            | 'Q' -> img "WhiteQueen.png"
            | 'p' -> img "BlackPawn.png"
            | 'b' -> img "BlackBishop.png"
            | 'n' -> img "BlackKnight.png"
            | 'r' -> img "BlackRook.png"
            | 'k' -> img "BlackKing.png"
            | 'q' -> img "BlackQueen.png"
            | _ -> failwith "invalid piece"
        
        /// creates file label
        let flbl i lbli = 
            let lbl = new Label()
            lbl.Text <- Lizard.Ref.fs.[i].ToString()
            lbl.Font <- new Font("Arial", 12.0F, FontStyle.Bold, GraphicsUnit.Point, byte (0))
            lbl.ForeColor <- Color.Green
            lbl.Height <- 21
            lbl.Width <- 42
            lbl.TextAlign <- ContentAlignment.MiddleCenter
            lbl.Left <- i * 42 + 30
            lbl.Top <- 8 * 42 + 24
            flbls.[i] <- lbl
        
        /// creates rank label
        let rlbl i lbli = 
            let lbl = new Label()
            lbl.Text <- (i + 1).ToString()
            lbl.Font <- new Font("Arial", 12.0F, FontStyle.Bold, GraphicsUnit.Point, byte (0))
            lbl.ForeColor <- Color.Green
            lbl.Height <- 42
            lbl.Width <- 21
            lbl.TextAlign <- ContentAlignment.MiddleCenter
            lbl.Left <- 0
            lbl.Top <- 7 * 42 - i * 42 + 16
            rlbls.[i] <- lbl
        
        /// Action for GiveFeedback
        let giveFeedback (e : GiveFeedbackEventArgs) = 
            e.UseDefaultCursors <- false
            sqpnl.Cursor <- cCur
        
        /// Action for Drag Over
        let dragOver (e : DragEventArgs) = e.Effect <- DragDropEffects.Move
        
        /// Action for Drag Drop
        let dragDrop (p : PictureBox, e) = 
            sqTo <- System.Convert.ToInt32(p.Tag)
            sqpnl.Cursor <- Cursors.Default
        
        /// Action for Mouse Down
        let mouseDown (p : PictureBox, e : MouseEventArgs) = 
            if e.Button = MouseButtons.Left then 
                let sqFrom = System.Convert.ToInt32(p.Tag)
                pstt.GetPossSqs(sqFrom)
                let oimg = p.Image
                p.Image <- null
                p.Refresh()
                let c = pstt.Pos.Sqs.[sqFrom]
                cCur <- getcur c
                sqpnl.Cursor <- cCur
                if pstt.PsSqs.Length>0 && (p.DoDragDrop(oimg, DragDropEffects.Move) = DragDropEffects.Move) then 
                    pstt.Move(sqFrom, sqTo)
                else p.Image <- oimg
                sqpnl.Cursor <- Cursors.Default
        ///set board colours and position of squares
        let setsq i sqi = 
            let r = i / 8
            let f = i % 8
            let sq = new PictureBox(Height = 42, Width = 42, SizeMode = PictureBoxSizeMode.CenterImage)
            sq.BackColor <- if (f + r) % 2 = 1 then Color.Green
                            else Color.PaleGreen
            sq.Left <- f * 42 + 1
            sq.Top <- r * 42 + 1
            sq.Tag <- i
            //events
            sq.MouseDown.Add(fun e -> mouseDown (sq, e))
            sq.DragDrop.Add(fun e -> dragDrop (sq, e))
            sq.AllowDrop <- true
            sq.DragOver.Add(dragOver)
            sq.GiveFeedback.Add(giveFeedback)
            sqs.[i] <- sq
        
        ///set pieces on squares
        let setpcs (p : Lizard.Pos) = 
            let setpcs() = p.Sqs |> Array.iteri (fun i c -> sqs.[i].Image <- if (c = ' ') then null else getim c)
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(setpcs)) |> ignore
                with _ -> ()
            else setpcs()
        
        ///orient board
        let orient isw = 
            let ori() = 
                let possq i (sq : PictureBox) = 
                    let r = i / 8
                    let f = i % 8
                    if not isw then 
                        sq.Top <- 7 * 42 - r * 42 + 1
                        sq.Left <- 7 * 42 - f * 42 + 1
                    else 
                        sq.Left <- f * 42 + 1
                        sq.Top <- r * 42 + 1
                sqs |> Array.iteri possq
                flbls |> Array.iteri (fun i l -> 
                             if isw then l.Left <- i * 42 + 30
                             else l.Left <- 7 * 42 - i * 42 + 30)
                rlbls |> Array.iteri (fun i l -> 
                             if isw then l.Top <- 7 * 42 - i * 42 + 16
                             else l.Top <- i * 42 + 16)
            if (this.InvokeRequired) then 
                try 
                    this.Invoke(MethodInvoker(ori)) |> ignore
                with _ -> ()
            else ori()
        
        ///highlight possible squares
        let highlightsqs sl = 
            sqs |> Array.iteri (fun i sq -> 
                       sqs.[i].BackColor <- if (i % 8 + i / 8) % 2 = 1 then Color.Green
                                            else Color.PaleGreen)
            sl |> List.iter (fun s -> 
                      sqs.[s].BackColor <- if (s % 8 + s / 8) % 2 = 1 then Color.YellowGreen
                                               else Color.Yellow)
        
        ///show promotion dialog
        let showprom (mv,isw) = 
            let dprom = new DlgProm(mv,isw)
            dprom.ShowDialog() |> ignore
        
        ///update bm
        let updbm (anl : Enganl) = 
            let txt = "Best Move: " + anl.BmPGN + " Score: " + anl.Scr.ToString()
            bmlbl.Text <- txt
        
        do 
            sqs |> Array.iteri setsq
            sqs |> Array.iter sqpnl.Controls.Add
            pstt.Pos |> setpcs
            edges |> List.iter this.Controls.Add
            flbls |> Array.iteri flbl
            flbls |> Array.iter this.Controls.Add
            rlbls |> Array.iteri rlbl
            rlbls |> Array.iter this.Controls.Add
            sqpnl |> this.Controls.Add
            bmlbl |> bmpnl.Controls.Add
            bmpnl |> this.Controls.Add
            pbtn |> btnpnl.Controls.Add
            fbtn |> btnpnl.Controls.Add
            btnpnl |> this.Controls.Add
            //events
            pstt.PosChng |> Observable.add setpcs
            pstt.PsSqsChng |> Observable.add highlightsqs
            pstt.Prom |> Observable.add showprom
            pstt.Ornt |> Observable.add orient
            pstt.BmChng |> Observable.add updbm
            pbtn.Click.Add(fun _ -> Clipboard.SetText({Lizard.PGN.Game.Blank() with Moves=pstt.Mvs}.ToString()))
            fbtn.Click.Add(fun _ -> Clipboard.SetText(pstt.Pos.ToString()))
    
    type Nav = 
        | Home
        | Prev
        | Next
        | End
        | NextL
        | PrevL
    
    type VarnGrid() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, Text = "Variation")
        let pnl = new Panel(Dock = DockStyle.Top, Height = 30)
        let nextmvs = 
            new ListView(Dock = DockStyle.Left, ForeColor = Color.ForestGreen, BackColor = SystemColors.Control, 
                         Width = 500)
        let homeb = new ToolStripButton(Image = img "homeButton.png")
        let prevb = new ToolStripButton(Image = img "prevButton.png")
        let nextb = new ToolStripButton(Image = img "nextButton.png")
        let endb = new ToolStripButton(Image = img "endButton.png")
        let sep = new ToolStripSeparator()
        let prevl = new ToolStripButton(Image = img "prevLineButton.png")
        let nextl = new ToolStripButton(Image = img "nextLineButton.png")
        let ts = 
            new ToolStrip(Anchor = AnchorStyles.Right, GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.None, 
                          Left = 100)
        let dg = 
            new Grid(Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, FixedRows = 1, FixedColumns = 1, 
                     EnableSort = false)
        let mutable hlcell:Option<int*int*Cells.Views.IView> = None
        let viewSelCell = new Cells.Views.Cell(BackColor = Color.Yellow)
        
        // updateLV
        let updateLV() = 
            let nmvs = vstt.GetNextMvs()
            nextmvs.Items.Clear()
            let addit (m : Move1) = 
                let itm = new ListViewItem()
                itm.Text <- m.Mpgn
                itm.Tag <- m.Mpgn
                nextmvs.Items.Add itm |> ignore
            nmvs |> List.iter addit

        //utility functions to convert from row column to variation and move
        let vm (r, c) = (c - 1) / 2, (r - 1) * 2 + (c - 1) % 2
        
        let rc (v, m) = 
            m / 2 + 1, 
            v * 2 + (if m % 2 = 1 then 1
                     else 0)
            + 1

        // updateVariation
        let updVarn (curv:Varn) = 
            let numcols = curv.Brchs.Length * 2
            let numrows = ((curv|>Varn.maxl) + 1 )/ 2
            dg.Rows.Clear()
            dg.Rows.Insert(0)
            dg.ColumnsCount <- numcols + 1
            //add header row
            dg.[0, 0] <- new Cells.ColumnHeader("")
            for c = 1 to numcols do
                let columnHeader = 
                    new Cells.ColumnHeader(if c % 2 = 1 then (c / 2 + 1).ToString() + "w"
                                           else (c / 2).ToString() + "b")
                columnHeader.View <- viewColHeader
                dg.[0, c] <- columnHeader
            // add row headers
            for r = 1 to numrows do
                dg.Rows.Insert(r)
                let rowheader = new SourceGrid.Cells.Cell(r, typedefof<string>)
                rowheader.View <- viewRowHeader
                dg.[r, 0] <- rowheader
            //add moves
            let brchs = curv.Brchs
            for b=0 to brchs.Length-1 do
                let brch = brchs.[b].Mvs
                let mutable ingrey = false
                for i = 0 to brch.Length-1 do
                    let cell = new SourceGrid.Cells.Cell(brch.[i].Mpgn, typedefof<string>)
                    ingrey <- 
                        b>0 && i=0 && brchs.[b].Mvs.[0].Mpgn=brchs.[b-1].Mvs.[0].Mpgn //initial set
                        || ingrey && brchs.[b].Mvs.[i].Mpgn=brchs.[b-1].Mvs.[i].Mpgn //already set and still the same 
                    let r,c = rc(b,i)
                    cell.View <- if c % 4 = 0 || (c + 1) % 4 = 0 then
                                    if ingrey then viewCell1G else viewCell1 
                                 else 
                                    if ingrey then viewCell2G else viewCell2
                    dg.[r,c] <- cell
            dg.AutoSizeCells()
        
        // selmv called when cell is selected
        let selmv (e : RangeRegionChangedEventArgs) = 
            if e.AddedRange <> null && e.AddedRange.Count = 1 && dg.RowsCount > 1 then 
                let cl = e.AddedRange.GetCellsPositions().[0]
                if cl.Row > 0 && cl.Column > 0 && cl.Row < dg.RowsCount - 2 then 
                    let v, m = vm (cl.Row, cl.Column)
                    vstt.GetPos(v, m)
                    //need to unhighlight previous one
                    if hlcell.IsSome then 
                        let r,c,vw = hlcell.Value
                        dg.[r,c].View <- vw
                    hlcell <- (cl.Row, cl.Column,dg.[cl.Row, cl.Column].View)|>Some
                    dg.[cl.Row, cl.Column].View <- viewSelCell
                    updateLV()
        
        // selcel called to select a defined cell given a variation and move
        let selcel (v, m) = 
            let r, c = rc (v, m)
            let p = new Position(r, c)
            dg.Selection.Focus(p, true) |> ignore
        
        //function to go to end
        let rec gotoend ri ci = 
            let nr, nc = 
                if ci % 2 = 0 then ri + 1, ci - 1
                else ri, ci + 1
            if nr = dg.RowsCount - 2 || dg.[nr, nc].Value = null then ri, ci
            else gotoend nr nc
        
        // do navigation
        let donav (n) = 
            let p = dg.Selection.ActivePosition
            if p <> Position.Empty then 
                let r = p.Row
                let c = p.Column
                
                let nr, nc = 
                    match n with
                    | Home -> 
                        if c % 2 = 0 then 1, c - 1
                        else 1, c
                    | Prev -> 
                        if c % 2 = 0 then r, c - 1
                        else r - 1, c + 1
                    | Next -> 
                        if c % 2 = 0 then r + 1, c - 1
                        else r, c + 1
                    | End -> gotoend r c
                    | PrevL -> r, c - 2
                    | NextL -> r, c + 2
                if nc < dg.ColumnsCount && nc > 0 && nr < dg.RowsCount - 2 && nr > 0 then 
                    let np = new Position(nr, nc)
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
        
        // next move click
        let mvslv (e) = 
            if (nextmvs.SelectedItems.Count = 1) then 
                let items = nextmvs.SelectedItems
                let mv = items.[0].Tag.ToString()
                vstt.DoNextMv(mv)
        
        do 
            this.Controls.Add(dg)
            [ homeb; prevb; nextb; endb ] |> List.iter (fun c -> ts.Items.Add(c) |> ignore)
            ts.Items.Add(sep) |> ignore
            [ prevl; nextl ] |> List.iter (fun c -> ts.Items.Add(c) |> ignore)
            pnl.Controls.Add(ts)
            pnl.Controls.Add(nextmvs)
            this.Controls.Add(pnl)
            //events
            dg.KeyDown.Add(dokeydown)
            dg.Selection.SelectionChanged.Add(selmv)
            vstt.CurChng |> Observable.add updVarn
            vstt.SelCell |> Observable.add selcel
            homeb.Click.Add(fun _ -> donav (Home))
            prevb.Click.Add(fun _ -> donav (Prev))
            nextb.Click.Add(fun _ -> donav (Next))
            endb.Click.Add(fun _ -> donav (End))
            prevl.Click.Add(fun _ -> donav (PrevL))
            nextl.Click.Add(fun _ -> donav (NextL))
            nextmvs.Click.Add(mvslv)
    
    type Anal() as this = 
        inherit DockContent(Icon = ico "anal.ico", CloseButtonVisible = false, Text = "Analysis Results")
        let pnl = new Panel(Dock = DockStyle.Top, Height = 30)
        let fp = new FlowLayoutPanel(Dock = DockStyle.Fill)
        let rtb = new RichTextBox(Dock = DockStyle.Fill)
        let scrcb = new System.Windows.Forms.CheckBox(Text = "score", Checked = true)
        let ln1 = new NumericUpDown(Width = 50, Value = 99m)
        let ln0 = new NumericUpDown(Width = 50, Value = 1m)
        let mv1 = new NumericUpDown(Width = 50, Value = 99m)
        let mv0 = new NumericUpDown(Width = 50, Value = 1m)
        let bcb = new System.Windows.Forms.CheckBox(Text = "Black", Checked = true)
        let wcb = new System.Windows.Forms.CheckBox(Text = "White", Checked = true)
        
        let getLine (bms : Engbm []) j line = 
            let doline = 
                (bcb.Checked && wcb.Checked) || (bcb.Checked && not bms.[j].Bisw) || (wcb.Checked && bms.[j].Bisw)
            if doline then 
                if (bms.[j].Bnum >= int (mv0.Value) && bms.[j].Bnum <= int (mv1.Value)) then 
                    if scrcb.Checked then line + " " + bms.[j].Bstr + "(" + bms.[j].Bscr.ToString() + ")"
                    else line + " " + bms.[j].Bstr
                else line
            else line
        
        let updAnl (e) = 
            let nl = System.Environment.NewLine
            let currBms = astt.CurrBms
            //clear current items
            rtb.Clear()
            //set heading font a write
            let fBold = new Font("Tahoma", 8.0f, FontStyle.Bold)
            let fNorm = new Font("Tahoma", 8.0f, FontStyle.Regular)
            rtb.SelectionFont <- fBold
            rtb.SelectionColor <- Color.DarkGreen
            rtb.SelectedText <- "Missing Best Moves by Line" + nl
            //load filtered results and write
            let mutable maxm = 0
            for i = 0 to currBms.Length - 1 do
                if (i + 1 >= int (ln0.Value) && i + 1 <= int (ln1.Value)) then 
                    let bms = currBms.[i]
                    let mutable line = (i + 1).ToString() + "."
                    for j = 0 to bms.Length - 1 do
                        maxm <- max maxm bms.[j].Bnum
                        //add items if satisfy filters
                        line <- getLine bms j line
                    rtb.SelectionFont <- fNorm
                    rtb.SelectionColor <- Color.Black
                    rtb.SelectedText <- line + nl
            //update range values
            mv1.Value <- min mv1.Value (decimal (maxm))
            ln1.Value <- min ln1.Value (decimal (currBms.Length))
        
        //reset filters
        let reset() = 
            [ wcb; bcb; scrcb ] |> List.iter (fun c -> c.Checked <- true)
            [ mv0; ln0 ] |> List.iter (fun c -> c.Value <- 1m)
            [ mv1; ln1 ] |> List.iter (fun c -> c.Value <- 99m)
            updAnl ("")
        
        do 
            fp.Controls.Add(wcb)
            fp.Controls.Add(bcb)
            fp.Controls.Add(new Label(Text = "moves", TextAlign = ContentAlignment.MiddleRight))
            fp.Controls.Add(mv0)
            fp.Controls.Add(mv1)
            fp.Controls.Add(new Label(Text = "lines", TextAlign = ContentAlignment.MiddleRight))
            fp.Controls.Add(ln0)
            fp.Controls.Add(ln1)
            fp.Controls.Add(new Label(Text = " ", TextAlign = ContentAlignment.MiddleRight))
            fp.Controls.Add(scrcb)
            this.Controls.Add(rtb)
            pnl.Controls.Add(fp)
            this.Controls.Add(pnl)
            [ wcb; bcb; scrcb ] |> List.iter (fun c -> c.CheckStateChanged.Add(updAnl))
            [ mv0; mv1; ln0; ln1 ] |> List.iter (fun c -> c.ValueChanged.Add(updAnl))
            vstt.CurChng |> Observable.add (fun _ -> reset())
    
    type Test() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, Text = "Test")
        let pnlb = new Panel(Dock = DockStyle.Bottom, Height = 30)
        let cbtn = new System.Windows.Forms.Button(Text = "Close", Dock = DockStyle.Right)
        let dg = 
            new Grid(Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, FixedRows = 1, FixedColumns = 1, 
                     EnableSort = false)
        
        //update tests
        let updTests (tsts : TestDet []) = 
            this.Focus() |> ignore
            dg.Rows.Clear()
            dg.Rows.Insert(0)
            dg.ColumnsCount <- 4
            let addhdr i txt = 
                let columnHeader = new Cells.ColumnHeader(txt)
                columnHeader.View <- viewColHeader
                dg.[0, i] <- columnHeader
            [ "No"; "Variation"; "MoveNum"; "Status" ] |> List.iteri addhdr
            let addcell r c cl = 
                dg.[r, c] <- new SourceGrid.Cells.Cell(cl, typedefof<string>)
                dg.[r, c].View <- viewCell2
            
            let addr r (tst : TestDet) = 
                dg.Rows.Insert(r + 1)
                let rowheader = new SourceGrid.Cells.Cell(r, typedefof<string>)
                rowheader.View <- viewRowHeader
                dg.[r + 1, 0] <- rowheader
                tst.Vnname |> addcell (r + 1) 1
                tst.Mvl.Length / 2 |> addcell (r + 1) 2
                tst.Status |> addcell (r + 1) 3
            
            tsts |> Array.iteri addr
            dg.AutoSizeCells()
            Cursor.Current <- Cursors.Default
        
        //update tests
        let updTest (numtest, status) = 
            dg.[numtest + 1, 3].Value <- status
            dg.[numtest + 1, 3].View <- if status = "Passed" then greenCell
                                        else redCell
            dg.Focus() |> ignore
            let nr = min (dg.RowsCount - 1) (numtest + 2)
            let np = new Position(nr, 1)
            dg.Selection.Focus(np, true) |> ignore
        
        // seltst called when cell is selected
        let seltst (e : RangeRegionChangedEventArgs) = 
            if e.AddedRange <> null && e.AddedRange.Count = 1 && dg.RowsCount > 1 then 
                let cl = e.AddedRange.GetCellsPositions().[0]
                if cl.Row > 0 then tstt.SetTestPos(cl.Row - 1)
        
        do 
            this.Controls.Add(dg)
            pnlb.Controls.Add(cbtn)
            this.Controls.Add(pnlb)
            //events
            tstt.TestChng |> Observable.add updTests
            dg.Selection.SelectionChanged.Add(seltst)
            tstt.TestRes |> Observable.add updTest
            cbtn.Click.Add(fun _ -> tstt.CloseTest())
    
    type TestRes() as this = 
        inherit DockContent(Icon = ico "board.ico", Text = "Test Results", HideOnClose = true)
        let pnlb = new Panel(Dock = DockStyle.Bottom, Height = 30)
        let cbtn = new System.Windows.Forms.Button(Text = "Close", Dock = DockStyle.Right)
        let dg = new Grid(Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        
        //update results
        let updRes (ress : string [] []) = 
            this.Focus() |> ignore
            dg.Rows.Clear()
            dg.Rows.Insert(0)
            dg.ColumnsCount <- 4
            let addhdr i txt = 
                let columnHeader = new Cells.ColumnHeader(txt)
                columnHeader.View <- viewColHeader
                dg.[0, i] <- columnHeader
            [ "Colour"; "Variation"; "Date"; "Score" ] |> List.iteri addhdr
            let addcell vc r c cl = 
                dg.[r, c] <- new SourceGrid.Cells.Cell(cl, typedefof<string>)
                dg.[r, c].View <- vc
            
            let addr r (res : string []) = 
                dg.Rows.Insert(r + 1)
                let vc = 
                    if res.[3] = "-" then viewCell2
                    elif int (res.[3]) > 89 then greenCell
                    else redCell
                res |> Array.iteri (addcell vc (r + 1))
            
            ress |> Array.iteri addr
            dg.AutoSizeCells()
        
        do 
            this.Controls.Add(dg)
            pnlb.Controls.Add(cbtn)
            this.Controls.Add(pnlb)
            //events
            tstt.ResLoad |> Observable.add updRes
            cbtn.Click.Add(fun _ -> this.Hide())

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
    
   
