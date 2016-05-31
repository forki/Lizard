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
    
    let viewColHeader = 
        new Cells.Views.ColumnHeader(Background = new ColumnHeader(BackColor = Color.Green))
    let viewRowHeader = 
        new Cells.Views.RowHeader(Background = new RowHeader(BackColor = Color.Green))
    let viewCell1 = new Cells.Views.Cell(BackColor = Color.LightGreen)
    let viewCell1G = 
        new Cells.Views.Cell(BackColor = Color.LightGreen, 
                             ForeColor = Color.Gray)
    let viewCell3 = new Cells.Views.Cell(BackColor = Color.LimeGreen)
    let viewCell2 = new Cells.Views.Cell(BackColor = Color.White)
    let viewCell2G = 
        new Cells.Views.Cell(BackColor = Color.White, ForeColor = Color.Gray)
    let greenCell = 
        new Cells.Views.Cell(BackColor = Color.White, 
                             ForeColor = Color.DarkGreen)
    let redCell = 
        new Cells.Views.Cell(BackColor = Color.White, ForeColor = Color.Red)
    
    let colhd txt = 
        let ans = new Cells.ColumnHeader(txt)
        ans.View <- viewColHeader
        ans
    
    let whtcl txt = 
        let ans = new Cells.Cell(txt)
        ans.View <- viewCell2
        ans
    
    let grncl txt = 
        let ans = new Cells.Cell(txt)
        ans.View <- viewCell1
        ans
    
    let wgrncl txt = 
        let ans = new Cells.Cell(txt)
        ans.View <- greenCell
        ans
    
    let pgrncl txt = 
        let ans = new Cells.Cell(txt)
        ans.View <- viewCell3
        ans
    
    ///Convert Source Grid grid (sg) to Array2d of cells
    let toArray2D (sg : Grid) = 
        let ans = Array2D.zeroCreate sg.RowsCount sg.ColumnsCount
        ans |> Array2D.mapi (fun r c _ -> sg.[r, c])