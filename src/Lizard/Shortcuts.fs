namespace LizardChessUI

open System.Drawing
open System.Windows.Forms

[<AutoOpen>]
module Shortcuts = 
    let img nm = 
        let thisExe = System.Reflection.Assembly.GetExecutingAssembly()
        let file = thisExe.GetManifestResourceStream(nm)
        Image.FromStream(file)
    
    let ico nm = 
        let thisExe = System.Reflection.Assembly.GetExecutingAssembly()
        let file = thisExe.GetManifestResourceStream(nm)
        new Icon(file)
    
    let cur nm = 
        let thisExe = System.Reflection.Assembly.GetExecutingAssembly()
        let file = thisExe.GetManifestResourceStream(nm)
        new Cursor(file)
    
    //SourceGrid shortcuts
    //views used
    open SourceGrid
    open DevAge.Drawing.VisualElements
    open DevAge.Drawing
    
    let viewColHeader = 
        new Cells.Views.ColumnHeader(Background = new ColumnHeader(BackColor = Color.Green))
    let viewColHeaderRed = 
        new Cells.Views.ColumnHeader(Background = new ColumnHeader(BackColor = Color.Green), ForeColor = Color.Red)
    let viewRowHeader = 
        new Cells.Views.RowHeader(Background = new RowHeader(BackColor = Color.Green))
    let viewCell1 = new Cells.Views.Cell(BackColor = Color.White,Border=RectangleBorder(BorderLine.Black1Width, BorderLine.NoBorder))
    let viewCell1G = 
        new Cells.Views.Cell(BackColor = Color.White, 
                             ForeColor = Color.Gray,
                             Border=RectangleBorder(BorderLine.Black1Width, BorderLine.NoBorder))
    let viewCell3 = new Cells.Views.Cell(BackColor = Color.LimeGreen)
    let viewCell2 = new Cells.Views.Cell(BackColor = Color.White)
    let viewCell2G = 
        new Cells.Views.Cell(BackColor = Color.White, ForeColor = Color.Gray)
    let greenCell = 
        new Cells.Views.Cell(BackColor = Color.White, 
                             ForeColor = Color.DarkGreen)
    let redCell = 
        new Cells.Views.Cell(BackColor = Color.White, ForeColor = Color.Red)
    let viewSelCell = new Cells.Views.Cell(BackColor = Color.Yellow)
   
