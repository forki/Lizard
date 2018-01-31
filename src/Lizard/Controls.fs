namespace LizardChessUI

open System.Drawing
open System.Windows.Forms
open WeifenLuo.WinFormsUI.Docking
open SourceGrid
open Dialogs
open State
open Lizard.Types
open Lizard

module Controls = 
    type Board() as this = 
        inherit DockContent(Icon = ico "board.ico", CloseButtonVisible = false, Text = "Variation: None Selected")
        let mutable sqTo = -1
        let mutable cCur = Cursors.Default
        let bdpnl = new Panel(Dock=DockStyle.Top,Height = 480)
        let sqpnl = new Panel(Width = 420, Height = 420, Left = 29, Top = 13)
        
        let edges = 
            [ new Panel(BackgroundImage = img "Back.jpg", Width = 342, Height = 8, Left = 24, Top = 6)
              new Panel(BackgroundImage = img "Back.jpg", Width = 8, Height = 350, Left = 24, Top = 8)
              new Panel(BackgroundImage = img "Back.jpg", Width = 8, Height = 350, Left = 366, Top = 6)
              new Panel(BackgroundImage = img "Back.jpg", Width = 342, Height = 8, Left = 32, Top = 350) ]
        
        let sqs : PictureBox [] = Array.zeroCreate 64
        let flbls : Label [] = Array.zeroCreate 8
        let rlbls : Label [] = Array.zeroCreate 8
        let dg = new Grid(Dock = DockStyle.Bottom, BorderStyle = BorderStyle.FixedSingle, Height=150)
        let btnpnl = new Panel(Width = 420, Height = 40, Dock=DockStyle.Bottom)
        let pbtn = new System.Windows.Forms.Button(Text="Copy PGN",Dock=DockStyle.Left)
        let fbtn = new System.Windows.Forms.Button(Text="Copy \"FEN\"",Dock=DockStyle.Left)
        let bklbl = new Label(Text="",Dock=DockStyle.Top)
        let lnlbl = new Label(Text="Line: None Selected",Dock=DockStyle.Top)
        
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

        //load results
        let setmv(mvl:Move list) =
            if mvl.Length>0 then
                let mv = mvl.[mvl.Length-1]
                dg.Rows.Clear()
                dg.ColumnsCount <- 2
                dg.Columns.[0].Width <-150
                dg.Columns.[1].Width <-200
                dg.Rows.Insert(0)
                dg.[0, 0] <- new SourceGrid.Cells.Cell("Move Type", typedefof<string>)
                dg.[0, 0].View <- viewRowHeader
                dg.[0, 1] <- new SourceGrid.Cells.Cell(mv.Meval, typedefof<string>)
                dg.Rows.Insert(1)
                dg.[1, 0] <- new SourceGrid.Cells.Cell("Score Depth 10", typedefof<string>)
                dg.[1, 0].View <- viewRowHeader
                dg.[1, 1] <- new SourceGrid.Cells.Cell(mv.Scr10, typedefof<string>)
                dg.Rows.Insert(2)
                dg.[2, 0] <- new SourceGrid.Cells.Cell("Score Depth 25", typedefof<string>)
                dg.[2, 0].View <- viewRowHeader
                dg.[2, 1] <- new SourceGrid.Cells.Cell(mv.Scr25, typedefof<string>)
                dg.Rows.Insert(3)
                dg.[3, 0] <- new SourceGrid.Cells.Cell("Best Response", typedefof<string>)
                dg.[3, 0].View <- viewRowHeader
                dg.[3, 1] <- new SourceGrid.Cells.Cell(mv.Bresp, typedefof<string>)
                dg.Rows.Insert(4)
                dg.[4, 0] <- new SourceGrid.Cells.Cell("FICS % Played", typedefof<string>)
                dg.[4, 0].View <- viewRowHeader
                dg.[4, 1] <- new SourceGrid.Cells.Cell(mv.FicsPc.ToString("P"), typedefof<string>)
                lnlbl.Text <- "Line: " + mv.ECO
         
        do 
            sqs |> Array.iteri setsq
            sqs |> Array.iter sqpnl.Controls.Add
            pstt.Pos |> setpcs
            edges |> List.iter bdpnl.Controls.Add
            flbls |> Array.iteri flbl
            flbls |> Array.iter bdpnl.Controls.Add
            rlbls |> Array.iteri rlbl
            rlbls |> Array.iter bdpnl.Controls.Add
            sqpnl |> bdpnl.Controls.Add
            bdpnl |>this.Controls.Add
            lnlbl |> this.Controls.Add
            bklbl |> this.Controls.Add

            dg |> this.Controls.Add
            pbtn |> btnpnl.Controls.Add
            fbtn |> btnpnl.Controls.Add
            btnpnl |> this.Controls.Add
            //events
            pstt.PosChng |> Observable.add setpcs
            pstt.PsSqsChng |> Observable.add highlightsqs
            pstt.Prom |> Observable.add showprom
            pstt.Ornt |> Observable.add orient
            pstt.MvsChng |> Observable.add setmv
            pbtn.Click.Add(fun _ -> Clipboard.SetText({Lizard.PGN.Game.Blank() with Moves=pstt.Mvs}.ToString()))
            fbtn.Click.Add(fun _ -> Clipboard.SetText(pstt.Pos.ToString()))
            vstt.CurChng |> Observable.add(fun v -> this.Text <- v.ECO)

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
        
        // updateLV
        let updateLV() = 
            let nmvs = vstt.GetNextMvs()
            nextmvs.Items.Clear()
            let addit (m : Move) = 
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
            this.Text<-curv.Name
            let numcols = curv.Lines.Length * 2
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
            let brchs = curv.Lines
            for b=0 to brchs.Length-1 do
                let brch = brchs.[b].Mvs
                let mutable ingrey = false
                for i = 0 to brch.Length-1 do
                    let mv = brch.[i]
                    let suf = 
                        match mv.Meval with
                        |Normal -> ""
                        |Weak -> "?"
                        |Excellent -> "!"
                        |Surprising -> "!?"
                    let cell = new SourceGrid.Cells.Cell(mv.Mpgn + suf, typedefof<string>)
                    ingrey <- 
                        b>0 && i=0 && brchs.[b].Mvs.[0].Mpgn=brchs.[b-1].Mvs.[0].Mpgn //initial set
                        || ingrey && brchs.[b].Mvs.[i].Mpgn=brchs.[b-1].Mvs.[i].Mpgn //already set and still the same 
                    let r,c = rc(b,i)
                    cell.View <- if c % 2 = 0 then
                                    if ingrey then viewCell1G else viewCell1 
                                 else 
                                    if ingrey then viewCell2G else viewCell2
                    dg.[r,c] <- cell
                //set header color if losing
                let lst = brch.Length-1
                let scr = brch.[lst].Scr25
                let _,c = rc(b,lst)
                if curv.Isw && scr>0 then
                    dg.[0,c].View <- viewColHeaderRed
                    dg.[0,c+1].View <- viewColHeaderRed
                if (not curv.Isw) && scr>0 then
                    dg.[0,c].View <- viewColHeaderRed
                    dg.[0,c-1].View <- viewColHeaderRed

            dg.AutoSizeCells()
            hlcell <- None
        
        // selmv called when cell is selected
        let selmv (e : RangeRegionChangedEventArgs) = 
            if e.AddedRange <> null && e.AddedRange.Count = 1 && dg.RowsCount > 1 then 
                let cl = e.AddedRange.GetCellsPositions().[0]
                if cl.Row > 0 && cl.Column > 0 then 
                    let v, m = vm (cl.Row, cl.Column)
                    vstt.GetPos(v, m)
                    //need to unhighlight previous one
                    if hlcell.IsSome then 
                        let r,c,vw = hlcell.Value
                        dg.[r,c].View <- vw
                    hlcell <- (cl.Row, cl.Column,dg.[cl.Row, cl.Column].View)|>Some
                    dg.[cl.Row, cl.Column].View <- viewSelCell
                    //highlight headers
                    dg.[cl.Row, 0].View <- viewColHeaderSel
                    dg.[0, cl.Column].View <- viewColHeaderSel
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
            if dg.[nr, nc] = null then ri, ci
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
                if nc < dg.ColumnsCount && nc > 0 && nr > 0 && dg.[nr, nc] <> null then 
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
    
    type TestRes(rnd:bool) as this = 
        inherit DockContent(Icon = (if rnd then ico "rosette.ico" else ico "rosette_blue.ico"), Text = (if rnd then "Random Test Results" else "Linear Test Results"))
        let pnlb = new Panel(Dock = DockStyle.Bottom, Height = 30)
        let clbl = new System.Windows.Forms.Label(Text = "Double Click Row to Run Test", Width = 200)
        let rlbtn = new System.Windows.Forms.Button(Text="ReLoad",Dock=DockStyle.Right)
        let dg = new Grid(Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle, FixedRows = 1)
        let mutable res = 
            if rnd then Lizard.Test.getallres()
            else Lizard.Test.getallreslin()
        
        //load results
        let loadRes() = 
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
            
            res |> Array.iteri addr
            dg.AutoSizeCells()
        
        let reloadRes() =
            res <-
                if rnd then Lizard.Test.getallres()
                else Lizard.Test.getallreslin()
            loadRes()
        
        let runtest(sender:Grid) =
            let r = sender.MouseCellPosition.Row
            let nm = dg.[r,1].DisplayText
            let isw=dg.[r,0].DisplayText="White"
            tstt.LoadTest(rnd, nm, isw)

        do 
            this.Controls.Add(dg)
            pnlb.Controls.AddRange([|clbl;rlbtn|])
            this.Controls.Add(pnlb)
            loadRes()
            //Events
            dg.DoubleClick.Add(fun _ -> runtest(dg))
            rlbtn.Click.Add(fun _ -> reloadRes())

    type PosAnal() as this = 
        inherit DockContent(Icon = ico "cog2.ico", Text = "Analyse Position", 
                            CloseButtonVisible = false)
        let pnlt = new Panel(Dock = DockStyle.Top, Height = 30)
        let albl = 
            new Label(Dock = DockStyle.Top, Text = "Stopped", 
                      TextAlign = ContentAlignment.BottomLeft)
        let sbtn = 
            new System.Windows.Forms.Button(Text = "Start", 
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

        //start or stop
        let startstop() =
            if sbtn.Text="Start" then
                sbtn.Text<-"Stop"
                astt.AnlpStart()
            else
                sbtn.Text<-"Start"
                astt.AnlpStop()
        
        do 
            this.Controls.Add(dg)
            pnlt.Controls.Add(albl)
            pnlt.Controls.Add(sbtn)
            this.Controls.Add(pnlt)
            //events
            astt.AnlpChng |> Observable.add setAnl
            astt.AnlpHeadChng |> Observable.add setHdr
            astt.AnlpMsg |> Observable.add addMsg
            sbtn.Click.Add(fun _ -> startstop())
    
   
