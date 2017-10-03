namespace LizardChessUI

open System.Windows.Forms
open System.Drawing
open SourceGrid
open State
open Lizard.Types

module Dialogs = 
    //dialog for Promotion
    type DlgProm(mv : Move, isw : bool) as this = 
        inherit Form(Text = "Select Piece", Height = 78, Width = 182, FormBorderStyle = FormBorderStyle.FixedToolWindow)
        let sqs : PictureBox [] = Array.zeroCreate 4
        
        let bpcims = 
            [ img "BlackQueen.png"
              img "BlackRook.png"
              img "BlackKnight.png"
              img "BlackBishop.png" ]
        
        let wpcims = 
            [ img "WhiteQueen.png"
              img "WhiteRook.png"
              img "WhiteKnight.png"
              img "WhiteBishop.png" ]
        
        ///set pieces on squares
        let setsq i (sq : PictureBox) = 
            let sq = new PictureBox(Height = 42, Width = 42, SizeMode = PictureBoxSizeMode.CenterImage)
            sq.BackColor <- if i % 2 = 0 then Color.Green
                            else Color.PaleGreen
            sq.Top <- 1
            sq.Left <- i * 42 + 1
            sq.Image <- if isw then wpcims.[i]
                        else bpcims.[i]
            //events
            let mts = [Prom('Q');Prom('R');Prom('N');Prom('B')]
            sq.Click.Add(fun e -> 
                pstt.Promote({mv with Mtyp=mts.[i]})
                this.Close())
            sqs.[i] <- sq
        
        do 
            sqs |> Array.iteri setsq
            sqs |> Array.iter this.Controls.Add
    
    type DlgNew() as this = 
        inherit Form(Text = "Create New Variation", Height = 110, Width = 280, 
                     FormBorderStyle = FormBorderStyle.FixedDialog)
        let vc = new TableLayoutPanel(Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2)
        let hc1 = new FlowLayoutPanel(FlowDirection = FlowDirection.LeftToRight, Height = 30, Width = 260)
        let hc2 = new FlowLayoutPanel(FlowDirection = FlowDirection.RightToLeft, Height = 30, Width = 260)
        let nm = new TextBox(Text = "Type Name Here", Width = 120)
        let col = new ComboBox()
        let okbtn = new Button(Text = "OK")
        let cnbtn = new Button(Text = "Cancel")
        
        let donew (e) = 
            vstt.NewVarn(nm.Text, col.SelectedIndex = 0)
            this.Close()
        
        do 
            this.MaximizeBox <- false
            this.MinimizeBox <- false
            this.ShowInTaskbar <- false
            this.StartPosition <- FormStartPosition.CenterParent
            [| box "White"
               box "Black" |]
            |> col.Items.AddRange
            col.SelectedIndex <- 0
            hc1.Controls.Add(nm)
            hc1.Controls.Add(col)
            hc2.Controls.Add(cnbtn)
            hc2.Controls.Add(okbtn)
            [ hc1; hc2 ] |> List.iteri (fun i c -> vc.Controls.Add(c, 0, i))
            this.Controls.Add(vc)
            //events
            cnbtn.Click.Add(fun _ -> this.Close())
            okbtn.Click.Add(donew)
    
    type DlgSaveAs() as this = 
        inherit Form(Text = "Save Variation As", Height = 110, Width = 200, 
                     FormBorderStyle = FormBorderStyle.FixedDialog)
        let vc = new TableLayoutPanel(Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2)
        let hc1 = new FlowLayoutPanel(FlowDirection = FlowDirection.LeftToRight, Height = 30, Width = 180)
        let hc2 = new FlowLayoutPanel(FlowDirection = FlowDirection.RightToLeft, Height = 30, Width = 180)
        let nm = new TextBox(Text = "Type Name Here", Width = 170)
        let okbtn = new Button(Text = "OK")
        let cnbtn = new Button(Text = "Cancel")
        
        let dosav (e) = 
            vstt.SaveAsVarn(nm.Text)
            this.Close()
        
        do 
            this.MaximizeBox <- false
            this.MinimizeBox <- false
            this.ShowInTaskbar <- false
            this.StartPosition <- FormStartPosition.CenterParent
            hc1.Controls.Add(nm)
            hc2.Controls.Add(cnbtn)
            hc2.Controls.Add(okbtn)
            [ hc1; hc2 ] |> List.iteri (fun i c -> vc.Controls.Add(c, 0, i))
            this.Controls.Add(vc)
            //events
            cnbtn.Click.Add(fun _ -> this.Close())
            okbtn.Click.Add(dosav)
    
    type RowTyp = 
        | Head
        | Text
        | Num
        | Bool
    
    type DlgOpts() as this = 
        inherit Form(Text = "Options", Height = 530, Width = 400, FormBorderStyle = FormBorderStyle.FixedDialog)
        let pnlb = new Panel(Dock = DockStyle.Bottom, Height = 30)
        let hc = new FlowLayoutPanel(FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill)
        let okbtn = new Button(Text = "OK")
        let cnbtn = new Button(Text = "Cancel")
        let mutable dg = new Grid(Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle)
        
        let load() = 
            dg <- new Grid(Dock = DockStyle.Fill, BorderStyle = BorderStyle.FixedSingle)
            let opts = stt.GetOpts()
            //dg.Rows.Clear()
            dg.Rows.InsertRange(0, 24)
            dg.ColumnsCount <- 2
            let numed = new Cells.Editors.NumericUpDown(typedefof<decimal>, 60M, 1m, 1m)
            let txted = new Cells.Editors.TextBox(typedefof<string>)
            let tfed = new Cells.Editors.ComboBox(typedefof<string>, [| "False"; "True" |], true)
            
            let setrw r (prp, vl, typ) = 
                match typ with
                | Head -> 
                    dg.[r, 0] <- colhd prp
                    dg.[r, 1] <- colhd vl
                | Text -> 
                    dg.[r, 0] <- grncl prp
                    dg.[r, 1] <- whtcl vl
                    dg.[r, 1].Editor <- txted
                | Num -> 
                    dg.[r, 0] <- grncl prp
                    dg.[r, 1] <- whtcl (int vl)
                    dg.[r, 1].Editor <- numed
                | Bool -> 
                    dg.[r, 0] <- grncl prp
                    dg.[r, 1] <- whtcl vl
                    dg.[r, 1].Editor <- tfed
            
            let dat = 
                [ "Data", "Folders", Head
                  "Openings Folder", opts.Opnfol, Text
                  "Test Folder", opts.Tstfol, Text
                  "Engine Folder", opts.Engfol, Text
                  "Test", "Parameters", Head
                  "Random - Num Positions", opts.Rnum.ToString(), Num
                  "Random - Skip Moves", opts.Rskip.ToString(), Num
                  "Linear - Num Lines", opts.Lnum.ToString(), Num
                  "Linear = Skip Moves", opts.Lskip.ToString(), Num
                  "Engine", "Parameters", Head
                  "Max Depth", opts.Emaxdepth.ToString(), Num
                  "Create Log", opts.Elog.ToString(), Bool ]
            
            dat |> List.iteri setrw
            dg.Columns.[1].MinimalWidth <- 140
            dg.AutoSizeCells()
        
        let dosav (e) = 
            let opts = 
                { Opnfol = dg.[1, 1].Value.ToString()
                  Tstfol = dg.[2, 1].Value.ToString()
                  Engfol = dg.[3, 1].Value.ToString()
                  Rnum = int (dg.[5, 1].Value.ToString())
                  Rskip = int (dg.[6, 1].Value.ToString())
                  Lnum = int (dg.[7, 1].Value.ToString())
                  Lskip = int (dg.[8, 1].Value.ToString())
                  Emaxdepth = int (dg.[10, 1].Value.ToString())
                  Elog = dg.[11, 1].Value.ToString().StartsWith("T") }
            stt.SaveOpts(opts)
            this.Close()
        
        do 
            this.MaximizeBox <- false
            this.MinimizeBox <- false
            this.ShowInTaskbar <- false
            this.StartPosition <- FormStartPosition.CenterParent
            hc.Controls.Add(cnbtn)
            hc.Controls.Add(okbtn)
            pnlb.Controls.Add(hc)
            this.Controls.Add(pnlb)
            //events
            cnbtn.Click.Add(fun _ -> this.Close())
            okbtn.Click.Add(dosav)
        
        override this.OnLoad(e) = 
            base.OnLoad(e)
            load()
            this.Controls.Add(dg)
