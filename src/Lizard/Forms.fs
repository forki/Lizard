namespace LizardChessUI

open System.Windows.Forms
open State
open Controls
open Dialogs
open WeifenLuo.WinFormsUI.Docking

module Forms = 
    type FrmMain() as this = 
        inherit Form(Text = "Lizard Chess", 
                     WindowState = FormWindowState.Maximized, 
                     Icon = ico "Lizard.ico", IsMdiContainer = true)
        let mm = 
            let m = new MenuStrip()
            //do file menue
            let fil = new ToolStripMenuItem("File")
            //do new
            let nw = new ToolStripMenuItem("New")
            nw.ShortcutKeys <- Keys.Control ||| Keys.N
            nw.Click.Add(fun _ -> (new DlgNew()).ShowDialog() |> ignore)
            fil.DropDownItems.Add(nw)|>ignore
            //do open
            let opn = new ToolStripMenuItem("Open")
            opn.ShortcutKeys <- Keys.Control ||| Keys.O
            opn.Click.Add(fun _ -> (new DlgOpn()).ShowDialog() |> ignore)
            fil.DropDownItems.Add(opn)|>ignore
            //do save
            let dosave (e) = 
                let msg = vstt.SaveVarn()
                MessageBox.Show(msg) |> ignore
            let sav = new ToolStripMenuItem("Save")
            sav.ShortcutKeys <- Keys.Control ||| Keys.S
            sav.Click.Add(dosave)
            fil.DropDownItems.Add(sav)|>ignore
            //do save as 
            let sava = new ToolStripMenuItem("Save As")
            sava.ShortcutKeys <- Keys.Control ||| Keys.A
            sava.Click.Add(fun _ -> (new DlgSaveAs()).ShowDialog() |> ignore)
            fil.DropDownItems.Add(sava)|>ignore
            //do delete
            let dodel (e) = 
                let nm, isw = vstt.CurVarn.Name, vstt.CurVarn.Isw
                if MessageBox.Show
                       ("Do you want to delete variation " + nm + "?", "Delete Variation", MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question) = DialogResult.Yes then 
                    vstt.DelVarn(nm, isw)
                    MessageBox.Show
                        ("Variation " + nm + " deleted!", "Delete Variation", MessageBoxButtons.OK, 
                         MessageBoxIcon.Exclamation) |> ignore
            let del = new ToolStripMenuItem("Delete")
            del.ShortcutKeys <- Keys.Control ||| Keys.D
            del.Click.Add(dodel)
            fil.DropDownItems.Add(del)|>ignore
            //do delete line
            let dodelline (e) = 
                let selvar = vstt.SelVar
                if selvar <> -1 
                   && (MessageBox.Show
                           ("Do you want to delete line " + (selvar + 1).ToString() + " ?", "Delete Line", 
                            MessageBoxButtons.YesNo) = DialogResult.Yes) then vstt.DelLine()
            let dell = new ToolStripMenuItem("Delete Line")
            dell.ShortcutKeys <- Keys.Control ||| Keys.L
            dell.Click.Add(dodelline)
            fil.DropDownItems.Add(dell)|>ignore
            m.Items.Add(fil)|>ignore
            //do about menu
            let abt = new ToolStripMenuItem("About")
            //do source code
            let src = new ToolStripMenuItem("Source Code")
            src.Click.Add(fun _ -> System.Diagnostics.Process.Start("https://github.com/pbbwfc/Lizard") |> ignore)
            abt.DropDownItems.Add(src)|>ignore
            //do conservation
            let cns = new ToolStripMenuItem("Conservation")
            cns.Click.Add(fun _ -> System.Diagnostics.Process.Start("http://www.arc-trust.org/") |> ignore)
            abt.DropDownItems.Add(cns)|>ignore
            m.Items.Add(abt)|>ignore
            m
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
            pan.Show(cont, DockState.DockBottomAutoHide)
            pan.Activate()
            vn.Show(cont, DockState.Document)
            tst.Show(cont, DockState.Document)
            lres.Show(cont, DockState.DockRightAutoHide)
            rres.Show(cont, DockState.DockRightAutoHide)
            this.Controls.Add(mm)
            //events
            stt.ModeChng |> Observable.add updTabs
            this.Shown.Add(hideTabs)
