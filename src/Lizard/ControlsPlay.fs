namespace LizardChessUI

open System.Drawing
open System.Windows.Forms
open WeifenLuo.WinFormsUI.Docking
open SourceGrid
open DevAge.Drawing.VisualElements
open Dialogs
open State
open Lizard.Types

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
    
   
