namespace LizardChessUI

open System.Windows.Forms
open State
open Controls
open WeifenLuo.WinFormsUI.Docking

module Forms = 
    type FrmMain() as this = 
        inherit Form(Text = "Lizard Chess", 
                     WindowState = FormWindowState.Maximized, 
                     Icon = ico "Lizard.ico", IsMdiContainer = true)
        let ts = new TsRib()
        let bd = new Board()
        let vn = new VarnGrid()
        let tst = new Test()
        let lres = new TestRes(false)
        let rres = new TestRes(true)
        let pan = new PosAnal()
        let cont = new DockPanel(Dock = DockStyle.Fill,DockLeftPortion=400.0,DockRightPortion=300.0)
        
        let updTabs (mode) = 
            cont.SuspendLayout(true)
            match mode with
            | DoVarn -> 
                vn.Show()
                tst.Hide()
            | DoTest -> 
                vn.Hide()
                tst.Show()
            cont.ResumeLayout(true, true)
            Application.DoEvents()
        
        let hideTabs (e) = 
            tst.Hide()
        
        do 
            this.Controls.Add(cont)
            bd.Show(cont, DockState.DockLeft)
            pan.Show(cont, DockState.DockBottom)
            pan.Activate()
            vn.Show(cont, DockState.Document)
            tst.Show(cont, DockState.Document)
            lres.Show(cont, DockState.DockRightAutoHide)
            rres.Show(cont, DockState.DockRightAutoHide)
            this.Controls.Add(ts)
            //events
            stt.ModeChng |> Observable.add updTabs
            this.Shown.Add(hideTabs)
