namespace LizardChessUI

open System
open System.Windows.Forms
open Forms

module Main = 
    [<STAThread>]
    Application.EnableVisualStyles()
    
    let frm = new FrmMain()
    
    Application.Run(frm)