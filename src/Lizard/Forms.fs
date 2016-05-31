namespace LizardChessUI

open System.Windows.Forms
open System.Drawing
open State
open Controls
open ControlsPlay
open WeifenLuo.WinFormsUI.Docking

module Forms = 
    type FrmMain() as this = 
        inherit Form(Text = "Lizard Chess", 
                     WindowState = FormWindowState.Maximized, 
                     Icon = ico "Lizard.ico", IsMdiContainer = true)
        let ts = new TsRib()
        let bd = new Board()
        let al = new Anal()
        let vn = new Varn()
        let tst = new Test()
        let gm = new Game()
        let fc = new Fics()
        let db = new Db()
        let res = new TestRes()
        let fdt = new FicsData()
        let lan = new LineAnal()
        let pan = new PosAnal()
        let cont = new DockPanel(Dock = DockStyle.Fill)
        
        let updTabs (mode) = 
            cont.SuspendLayout(true)
            match mode with
            | DoVarn -> 
                vn.Show()
                tst.Hide()
                gm.Hide()
                fc.Hide()
                db.Hide()
            | DoTest -> 
                vn.Hide()
                tst.Show()
                gm.Hide()
                fc.Hide()
                db.Hide()
            | DoPlay -> 
                vn.Hide()
                tst.Hide()
                gm.Show()
                fc.Hide()
                db.Hide()
            | DoFics -> 
                vn.Hide()
                tst.Hide()
                gm.Hide()
                fc.Show()
                db.Hide()
            | DoDb -> 
                vn.Hide()
                tst.Hide()
                gm.Hide()
                fc.Hide()
                db.Show()
            cont.ResumeLayout(true, true)
            Application.DoEvents()
        
        let hideTabs (e) = 
            tst.Hide()
            res.Hide()
            gm.Hide()
            fc.Hide()
            db.Hide()
        
        do 
            this.Controls.Add(cont)
            bd.Show(cont, DockState.DockLeft)
            fdt.Show(cont, DockState.DockBottom)
            al.Show(cont, DockState.DockBottom)
            lan.Show(cont, DockState.DockBottom)
            pan.Show(cont, DockState.DockBottom)
            fdt.Activate()
            vn.Show(cont, DockState.Document)
            tst.Show(cont, DockState.Document)
            res.Show(cont, DockState.Document)
            gm.Show(cont, DockState.Document)
            fc.Show(cont, DockState.Document)
            db.Show(cont, DockState.Document)
            this.Controls.Add(ts)
            //events
            stt.ModeChng |> Observable.add updTabs
            tstt.ResTabLoad |> Observable.add (res.Show)
            this.Shown.Add(hideTabs)